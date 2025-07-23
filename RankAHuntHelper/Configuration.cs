using Dalamud.Configuration;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;
using static RankAHuntHelper.StaticData.ExpansionData;

namespace RankAHuntHelper;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool EnableNotice { get; set; } = false;
    public bool EnableCrossWorld { get; set; } = false;
    public string selectedWorldName { get; set; } = string.Empty;
    public Dictionary<Expansion, bool> SelectedExpansion { get; set; } = new();
    public Dictionary<Expansion, Dictionary<string, bool>> SelectedMap { get; set; } = new();
    public void Save() => Svc.PluginInterface.SavePluginConfig(this);
}
