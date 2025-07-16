using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.Logging;
using Lumina.Excel.Sheets;
using System;

namespace RankAHuntTrainAssistant.Data;

public static class MapInfo
{
    public static uint TerritoryId { get; private set; } = 0;
    public static string MapName { get; private set; } = "地图未初始化";
    public static int Instance { get; private set; } = -1;
    public static string InstanceName => Instance > 0 ? Instance.ToString() : "无";

    private static bool isBuilt = false;

    static MapInfo()
    {
        Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
        Svc.Framework.Update += OnFrameworkUpdate;
    }

    private static void OnFrameworkUpdate(IFramework framework)
    {
        if (!isBuilt && Svc.ClientState.LocalPlayer != null)
        {
            Build();
            isBuilt = true;
        }
    }

    private static void OnTerritoryChanged(ushort newTerritory)
    {
        Build();
    }

    public static void Build()
    {
        TerritoryId = Svc.ClientState.TerritoryType;
        MapName = Svc.Data.GetExcelSheet<TerritoryType>()?
            .GetRow(TerritoryId).PlaceName.Value.Name.ToString() ?? "未知地图";

        Instance = IpcService.Lifestream.GetCurrentInstanceIndex();
    }
}
