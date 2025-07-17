using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using Lumina.Excel.Sheets;
using System;
using System.Linq;

namespace RankAHuntTrainAssistant.Manager;

public static class MapManager
{   
    public static uint DcId { get; private set; } = 0;
    public static string DcName { get; private set; } = "数据中心未初始化";
    public static uint WorldId { get; private set; } = 0;
    public static string WorldName { get; private set; } = "世界未初始化";
    public static string[] WorldsList { get; private set; } = Array.Empty<string>(); 
    public static uint MapId { get; private set; } = 0;
    public static string MapName { get; private set; } = "地图未初始化";
    public static int Instance { get; private set; } = -1;
    public static string InstanceName => Instance > 0 ? Instance.ToString() : "无分线";

    private static bool IsBuilt = false;

    private static uint? LastWorldId = null;


    static MapManager()
    {
        Svc.Framework.Update += OnFrameworkUpdate;
        Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
    }

    private static void OnFrameworkUpdate(IFramework framework)
    {
        if (Svc.ClientState.LocalPlayer?.CurrentWorld.Value.DataCenter.Value == null) return;

        if (!IsBuilt)
        {
            BuildDcWorlds();
            BuildMap();
            IsBuilt = true;
        }

        if (Svc.ClientState.LocalPlayer?.CurrentWorld.RowId != LastWorldId)
        {
            BuildDcWorlds();
        }

    }

    private static void OnTerritoryChanged(ushort newTerritory)
    {
        BuildMap();
    }

    private static void BuildDcWorlds()
    {
        var player = Svc.ClientState.LocalPlayer;
        if (player?.CurrentWorld.Value.DataCenter.Value == null) return;

        var world = player.CurrentWorld.Value;
        var dc = world.DataCenter.Value;

        DcId = dc.RowId;
        DcName = dc.Name.ToString();
        WorldId = world.RowId;
        WorldName = world.Name.ToString();

        WorldsList = Svc.Data.GetExcelSheet<World>()?
            .Where(w => w.DataCenter.Value.RowId == DcId && w.RowId > 1000)
            .Select(w => w.Name.ToString())
            .Order()
            .ToArray() ?? Array.Empty<string>();

        LastWorldId = Svc.ClientState.LocalPlayer?.CurrentWorld.RowId;
    }


    public static void BuildMap()
    {
        MapId = Svc.ClientState.TerritoryType;
        MapName = Svc.Data.GetExcelSheet<TerritoryType>()?
            .GetRow(MapId).PlaceName.Value.Name.ToString() ?? "未知地图";
        Instance = IpcService.Lifestream.GetCurrentInstanceIndex();
    }

    public static void Refresh()
    {
        BuildDcWorlds();
        BuildMap();
    }


}

