using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace RankAHuntHelper.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(RankAHuntHelper plugin) : base("RankAHuntHelper###ahtcfg", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Configuration = RankAHuntHelper.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {

    }
}
