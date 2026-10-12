using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace PartyMarkerPartyList;

/// <summary>
/// Displays native player target markers beside party-list job icons.
/// </summary>
public sealed class Plugin : IDalamudPlugin
{
    private const string Command = "/partymarkers";

    private readonly ICommandManager commandManager;
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;
    private readonly PartyMarkerOverlay overlay;
    private readonly WindowSystem windowSystem = new("PartyMarkerPartyList");
    private readonly ConfigWindow configWindow;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    public Plugin(
        IDalamudPluginInterface pluginInterface,
        IGameGui gameGui,
        ITextureProvider textureProvider,
        IDataManager dataManager,
        ICommandManager commandManager,
        IChatGui chatGui,
        IPluginLog log)
    {
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;
        this.chatGui = chatGui;
        this.configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.overlay = new PartyMarkerOverlay(this.configuration, gameGui, textureProvider, dataManager, log);
        this.configWindow = new ConfigWindow(this.configuration, this.SaveConfiguration);
        this.windowSystem.AddWindow(this.configWindow);

        this.pluginInterface.UiBuilder.Draw += this.overlay.Draw;
        this.pluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.pluginInterface.UiBuilder.OpenConfigUi += this.configWindow.Toggle;

        this.commandManager.AddHandler(
            Command,
            new CommandInfo(this.OnCommand)
            {
                HelpMessage = "切换队伍列表头顶标记显示；on / off 开关，config 打开设置。",
            });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.commandManager.RemoveHandler(Command);
        this.pluginInterface.UiBuilder.OpenConfigUi -= this.configWindow.Toggle;
        this.pluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
        this.pluginInterface.UiBuilder.Draw -= this.overlay.Draw;
        this.windowSystem.RemoveAllWindows();
        this.overlay.Dispose();
    }

    private void SaveConfiguration()
    {
        this.pluginInterface.SavePluginConfig(this.configuration);
    }

    private void OnCommand(string command, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "":
            case "toggle":
                this.SetEnabled(!this.configuration.Enabled);
                break;
            case "on":
                this.SetEnabled(true);
                break;
            case "off":
                this.SetEnabled(false);
                break;
            case "config":
            case "settings":
                this.configWindow.Toggle();
                break;
            default:
                this.chatGui.PrintError($"未知参数：{args}。用法：{Command} [on|off|config]");
                break;
        }
    }

    private void SetEnabled(bool enabled)
    {
        this.configuration.Enabled = enabled;
        this.SaveConfiguration();
        this.chatGui.Print($"队伍列表头顶标记：{(enabled ? "已开启" : "已关闭")}");
    }
}
