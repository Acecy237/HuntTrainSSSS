using Dalamud.Configuration;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;

namespace RankAHuntTrainAssistant;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool EnableCrossWorld = false;
    public Dictionary<string, bool> WorldToggles = new();
    public Dictionary<string, bool> VersionSelections = new();
    public Dictionary<string, Dictionary<string, bool>> MapSelections = new();

    public void Save() => Svc.PluginInterface.SavePluginConfig(this);
}
