using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace PartyMarkerPartyList;

/// <summary>
/// Displays native player target markers beside party-list job icons.
/// </summary>
public sealed class Plugin : IDalamudPlugin
{
    private const string ToggleCommand = "/partymarkers";

    private readonly ICommandManager commandManager;
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly PartyMarkerOverlay overlay;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    public Plugin(
        IDalamudPluginInterface pluginInterface,
        IGameGui gameGui,
        ITextureProvider textureProvider,
        ICommandManager commandManager,
        IPluginLog log)
    {
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;
        this.overlay = new PartyMarkerOverlay(gameGui, textureProvider, log);

        this.pluginInterface.UiBuilder.Draw += this.overlay.Draw;

        this.commandManager.AddHandler(
            ToggleCommand,
            new CommandInfo(this.OnCommand)
            {
                HelpMessage = "切换队伍列表头顶标记显示。",
            });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.commandManager.RemoveHandler(ToggleCommand);
        this.pluginInterface.UiBuilder.Draw -= this.overlay.Draw;
        this.overlay.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        this.overlay.Toggle();
    }
}
