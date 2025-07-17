using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace RankAHuntTrainAssistant.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base("RankAHuntTrainAssistant###ahtcfg", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Configuration = Plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {

    }
}
