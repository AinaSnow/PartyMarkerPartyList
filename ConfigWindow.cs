using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace PartyMarkerPartyList;

/// <summary>
/// Settings window opened from the plugin installer or <c>/partymarkers config</c>.
/// </summary>
internal sealed class ConfigWindow : Window
{
    private readonly Configuration configuration;
    private readonly Action save;

    public ConfigWindow(Configuration configuration, Action save)
        : base("Party Marker Party List 设置###PartyMarkerPartyListConfig", ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.configuration = configuration;
        this.save = save;
    }

    /// <inheritdoc/>
    public override void Draw()
    {
        var changed = false;

        var enabled = this.configuration.Enabled;
        if (ImGui.Checkbox("显示标记", ref enabled))
        {
            this.configuration.Enabled = enabled;
            changed = true;
        }

        var showTrust = this.configuration.ShowTrustMembers;
        if (ImGui.Checkbox("亲信战友 / 剧情辅助器小队也显示", ref showTrust))
        {
            this.configuration.ShowTrustMembers = showTrust;
            changed = true;
        }

        var followAlpha = this.configuration.FollowPartyListAlpha;
        if (ImGui.Checkbox("跟随小队列表透明度", ref followAlpha))
        {
            this.configuration.FollowPartyListAlpha = followAlpha;
            changed = true;
        }

        var side = (int)this.configuration.Side;
        ImGui.TextUnformatted("位置");
        ImGui.SameLine();
        changed |= ImGui.RadioButton("职业图标左侧", ref side, (int)MarkerSide.Left);
        ImGui.SameLine();
        changed |= ImGui.RadioButton("职业图标右侧", ref side, (int)MarkerSide.Right);
        this.configuration.Side = (MarkerSide)side;

        ImGui.SetNextItemWidth(200.0f);
        var sizeScale = this.configuration.SizeScale;
        if (ImGui.SliderFloat("大小", ref sizeScale, 0.5f, 1.5f, "%.2f"))
        {
            this.configuration.SizeScale = sizeScale;
            changed = true;
        }

        ImGui.SetNextItemWidth(200.0f);
        var offset = this.configuration.Offset;
        if (ImGui.DragFloat2("偏移 (X, Y)", ref offset, 0.25f, -100.0f, 100.0f, "%.1f"))
        {
            this.configuration.Offset = offset;
            changed = true;
        }

        if (ImGui.Button("恢复默认位置"))
        {
            this.configuration.Side = MarkerSide.Left;
            this.configuration.SizeScale = 1.0f;
            this.configuration.Offset = Vector2.Zero;
            changed = true;
        }

        if (changed)
            this.save();
    }
}
