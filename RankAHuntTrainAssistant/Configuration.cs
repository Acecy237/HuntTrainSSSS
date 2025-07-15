using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using ECommons.DalamudServices;

namespace RankAHuntTrainAssistant;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
