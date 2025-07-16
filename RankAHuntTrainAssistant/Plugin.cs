using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.DalamudServices;
using ECommons.EzIpcManager;
using RankAHuntTrainAssistant.Services;
using RankAHuntTrainAssistant.windows;


namespace RankAHuntTrainAssistant;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/aht";

    // 用于读取和保存插件配置
    public static Configuration Configuration = new();

    // UI窗口管理器
    public readonly WindowSystem WindowSystem = new("RankAHuntTrainAssistant");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    // 构造函数
    public Plugin(IDalamudPluginInterface pluginInterface) 
    {
        ECommonsMain.Init(pluginInterface, this);
        IpcService.InitAll();

        Configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Svc.Commands.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "打开设置"
        });

        Svc.PluginInterface.UiBuilder.Draw += DrawUI;

        Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        Svc.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        ECommonsMain.Dispose();

        Svc.Commands.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
