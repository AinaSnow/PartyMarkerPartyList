using System.Diagnostics;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Arrays;
using FFXIVClientStructs.FFXIV.Component.GUI;

using Lumina.Excel.Sheets;

namespace PartyMarkerPartyList;

/// <summary>
/// Reads native player target markers and paints non-interactive icons over the party list.
/// </summary>
internal sealed unsafe class PartyMarkerOverlay : IDisposable
{
    private const float MarkerGapRatio = 0.12f;
    private const float FallbackIconSize = 20.0f;
    private const int MarkerSlotCount = 17;
    private static readonly TimeSpan RepeatedErrorLogInterval = TimeSpan.FromSeconds(30);

    private readonly Configuration configuration;
    private readonly IGameGui gameGui;
    private readonly ITextureProvider textureProvider;
    private readonly IPluginLog log;
    private readonly uint[] markerIconIds;
    private readonly Stopwatch errorClock = Stopwatch.StartNew();

    private string? lastErrorKey;
    private TimeSpan lastErrorLoggedAt;
    private int suppressedErrors;
    private bool disposed;

    public PartyMarkerOverlay(
        Configuration configuration,
        IGameGui gameGui,
        ITextureProvider textureProvider,
        IDataManager dataManager,
        IPluginLog log)
    {
        this.configuration = configuration;
        this.gameGui = gameGui;
        this.textureProvider = textureProvider;
        this.log = log;
        this.markerIconIds = LoadMarkerIconIds(dataManager, log);
    }

    /// <summary>
    /// Draws icons at the native job-icon screen positions.
    /// </summary>
    public void Draw()
    {
        if (this.disposed || !this.configuration.Enabled || this.gameGui.GameUiHidden)
            return;

        try
        {
            this.DrawCore();
        }
        catch (Exception ex)
        {
            // Rendering is called every frame. Keep a transient native/UI state issue from breaking the plugin loop,
            // and don't flood the log when the same failure repeats every frame.
            this.LogDrawError(ex);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.disposed = true;
    }

    private static uint[] LoadMarkerIconIds(IDataManager dataManager, IPluginLog log)
    {
        try
        {
            var sheet = dataManager.GetExcelSheet<Marker>();
            return MarkerMatcher.BuildIconTable(
                MarkerSlotCount,
                rowId => sheet.TryGetRow(rowId, out var row) && row.Icon > 0 ? (uint)row.Icon : 0);
        }
        catch (Exception ex)
        {
            log.Warning(ex, "Failed to read the Marker sheet; using built-in marker icons.");
            return MarkerMatcher.DefaultTargetMarkerIconIds;
        }
    }

    /// <summary>
    /// Accumulates scale and alpha from the node up through the addon root, which carries the HUD layout scale and
    /// the party list's fade/opacity.
    /// </summary>
    private static void GetWorldScaleAndAlpha(AtkResNode* node, out Vector2 scale, out float alpha)
    {
        scale = Vector2.One;
        alpha = 1.0f;
        for (var depth = 0; node != null && depth < 32; depth++, node = node->ParentNode)
        {
            scale *= new Vector2(MathF.Abs(node->ScaleX), MathF.Abs(node->ScaleY));
            alpha *= node->Color.A / 255.0f;
        }
    }

    private void DrawCore()
    {
        var addon = this.gameGui.GetAddonByName<AddonPartyList>("_PartyList");
        if (addon == null || !addon->IsReady || !addon->IsVisible)
            return;

        var numberArray = PartyListNumberArray.Instance();
        if (numberArray == null)
            return;

        var markingController = MarkingController.Instance();
        if (markingController == null)
            return;

        var markers = markingController->Markers;
        var drawList = ImGui.GetBackgroundDrawList();

        var partyNumbers = numberArray->PartyMembers;
        var partyRows = addon->PartyMembers;
        var partyCount = Math.Clamp(numberArray->PartyListCount, 0, partyNumbers.Length);
        for (var slot = 0; slot < partyCount; slot++)
        {
            var member = partyNumbers[slot];
            this.DrawMember(drawList, markers, partyRows, member.DisplayRow, member.EntityId);
        }

        if (this.configuration.ShowTrustMembers)
        {
            var trustNumbers = numberArray->TrustMembers;
            var trustRows = addon->TrustMembers;
            var trustCount = Math.Clamp(numberArray->TrustCount, 0, trustNumbers.Length);
            for (var slot = 0; slot < trustCount; slot++)
            {
                // Trust rows have their own node array, filled in the same order as the number array.
                this.DrawMember(drawList, markers, trustRows, slot, trustNumbers[slot].EntityId);
            }
        }
    }

    private void DrawMember(
        ImDrawListPtr drawList,
        ReadOnlySpan<FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectId> markers,
        Span<AddonPartyList.PartyListMemberStruct> rows,
        int row,
        uint entityId)
    {
        if (!MarkerMatcher.TryGetMarkerIconId(markers, entityId, this.markerIconIds, out var markerIconId))
            return;

        if ((uint)row >= (uint)rows.Length)
            return;

        var jobIcon = rows[row].ClassJobIcon;
        if (jobIcon == null || !jobIcon->IsVisible())
            return;

        if (!this.textureProvider.TryGetFromGameIcon(new GameIconLookup(markerIconId), out var sharedTexture) ||
            !sharedTexture.TryGetWrap(out var texture, out _))
        {
            return;
        }

        GetWorldScaleAndAlpha((AtkResNode*)jobIcon, out var scale, out var alpha);
        if (!this.configuration.FollowPartyListAlpha)
            alpha = 1.0f;

        if (alpha <= 0.0f)
            return;

        var iconWidth = jobIcon->Width * scale.X;
        var iconHeight = jobIcon->Height * scale.Y;
        if (iconWidth <= 0.0f || iconHeight <= 0.0f)
        {
            iconWidth = FallbackIconSize;
            iconHeight = FallbackIconSize;
        }

        var markerSize = MathF.Min(iconWidth, iconHeight) * Math.Clamp(this.configuration.SizeScale, 0.1f, 4.0f);
        var gap = MathF.Max(1.0f, markerSize * MarkerGapRatio);
        var x = this.configuration.Side == MarkerSide.Right
            ? jobIcon->ScreenX + iconWidth + gap
            : jobIcon->ScreenX - markerSize - gap;
        var markerPosition = new Vector2(x, jobIcon->ScreenY + ((iconHeight - markerSize) * 0.5f))
            + (this.configuration.Offset * scale);

        drawList.AddImage(
            texture.Handle,
            markerPosition,
            markerPosition + new Vector2(markerSize),
            Vector2.Zero,
            Vector2.One,
            ImGui.GetColorU32(new Vector4(1.0f, 1.0f, 1.0f, Math.Clamp(alpha, 0.0f, 1.0f))));
    }

    private void LogDrawError(Exception ex)
    {
        var key = $"{ex.GetType().FullName}: {ex.Message}";
        var now = this.errorClock.Elapsed;
        if (key == this.lastErrorKey && now - this.lastErrorLoggedAt < RepeatedErrorLogInterval)
        {
            this.suppressedErrors++;
            return;
        }

        if (this.suppressedErrors > 0)
            this.log.Error($"Previous overlay error repeated {this.suppressedErrors} more time(s).");

        this.log.Error(ex, "Party Marker Party List failed to draw an overlay frame.");
        this.lastErrorKey = key;
        this.lastErrorLoggedAt = now;
        this.suppressedErrors = 0;
    }
}
