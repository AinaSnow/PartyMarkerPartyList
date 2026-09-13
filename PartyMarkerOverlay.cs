using System.Numerics;
using System.Threading;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Arrays;

namespace PartyMarkerPartyList;

/// <summary>
/// Reads native player target markers and paints non-interactive icons over the party list.
/// </summary>
internal sealed unsafe class PartyMarkerOverlay : IDisposable
{
    private const int PartyMemberCapacity = 8;
    private const nint PartyMembersOffset = 0x238;
    private const nint PartyNumberMembersOffset = 0x20;
    private const float MarkerGapRatio = 0.12f;
    private const float FallbackIconSize = 20.0f;

    private readonly IGameGui gameGui;
    private readonly ITextureProvider textureProvider;
    private readonly IPluginLog log;

    private bool enabled = true;
    private bool disposed;

    public PartyMarkerOverlay(
        IGameGui gameGui,
        ITextureProvider textureProvider,
        IPluginLog log)
    {
        this.gameGui = gameGui;
        this.textureProvider = textureProvider;
        this.log = log;
    }

    /// <summary>
    /// Toggles the overlay from the in-game command.
    /// </summary>
    public void Toggle()
    {
        Volatile.Write(ref this.enabled, !Volatile.Read(ref this.enabled));
    }

    /// <summary>
    /// Draws icons at the native job-icon screen positions.
    /// </summary>
    public void Draw()
    {
        if (this.disposed || !Volatile.Read(ref this.enabled) || this.gameGui.GameUiHidden)
            return;

        try
        {
            this.DrawCore();
        }
        catch (Exception ex)
        {
            // Rendering is called every frame. Keep a transient native/UI state issue from breaking the plugin loop.
            this.log.Error(ex, "Party Marker Party List failed to draw an overlay frame.");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.disposed = true;
    }

    private void DrawCore()
    {
        var addon = this.gameGui.GetAddonByName<AddonPartyList>("_PartyList");
        if (addon == null || !addon->IsReady || !addon->IsVisible)
            return;

        var numberArray = PartyListNumberArray.Instance();
        if (numberArray == null || numberArray->PartyListCount <= 0)
            return;

        var markingController = MarkingController.Instance();
        if (markingController == null)
            return;

        var nativeMembers = (PartyListNumberArray.PartyListMemberNumberArray*)((byte*)numberArray + PartyNumberMembersOffset);
        var addonMembers = (AddonPartyList.PartyListMemberStruct*)((byte*)addon + PartyMembersOffset);
        var drawList = ImGui.GetForegroundDrawList();
        var memberCount = Math.Min(numberArray->PartyListCount, PartyMemberCapacity);

        for (var slot = 0; slot < memberCount; slot++)
        {
            var nativeMember = nativeMembers[slot];
            if (!MarkerMatcher.TryGetMarkerIconId(markingController->Markers, nativeMember.EntityId, out var markerIconId))
            {
                continue;
            }

            var row = nativeMember.DisplayRow;
            if ((uint)row >= PartyMemberCapacity)
                continue;

            var jobIcon = addonMembers[row].ClassJobIcon;
            if (jobIcon == null || !jobIcon->IsVisible())
                continue;

            if (!this.textureProvider.TryGetFromGameIcon(
                    new GameIconLookup((uint)markerIconId),
                    out var sharedTexture) ||
                !sharedTexture.TryGetWrap(out var texture, out _))
            {
                continue;
            }

            var iconWidth = jobIcon->Width * MathF.Abs(jobIcon->ScaleX);
            var iconHeight = jobIcon->Height * MathF.Abs(jobIcon->ScaleY);
            if (iconWidth <= 0.0f || iconHeight <= 0.0f)
            {
                iconWidth = FallbackIconSize;
                iconHeight = FallbackIconSize;
            }

            var markerSize = MathF.Min(iconWidth, iconHeight);
            var markerPosition = new Vector2(
                jobIcon->ScreenX - markerSize - MathF.Max(1.0f, markerSize * MarkerGapRatio),
                jobIcon->ScreenY + (iconHeight - markerSize) * 0.5f);

            drawList.AddImage(
                texture.Handle,
                markerPosition,
                markerPosition + new Vector2(markerSize));
        }
    }
}
