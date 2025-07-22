using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons.Singletons;
using ECommons;
using RankAHuntHelper.Windows;


namespace RankAHuntHelper;

public sealed class RankAHuntHelper : IDalamudPlugin
{
    private const string CommandName = "/ahh";

    public static Configuration Configuration = new();

    public readonly WindowSystem WindowSystem = new("RankAHuntHelper");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public RankAHuntHelper(IDalamudPluginInterface pluginInterface) 
    {
        ECommonsMain.Init(pluginInterface, this);

        SingletonServiceManager.Initialize(typeof(ServiceManager));

        Configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Svc.Commands.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "打开主界面"
        });

        Svc.PluginInterface.UiBuilder.Draw += DrawUI;

        Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        Svc.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        ToggleMainUI();
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
