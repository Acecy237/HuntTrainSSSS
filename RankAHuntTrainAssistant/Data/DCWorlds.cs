using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.Logging;
using Lumina.Excel.Sheets;
using System;
using System.Linq;

namespace RankAHuntTrainAssistant.Data;

public static class DCWorlds
{
    public static uint? DcId { get; private set; } = null;
    public static string DcName { get; private set; } = "数据中心未初始化";
    public static string WorldName { get; private set; } = "世界未初始化";
    public static string[] WorldsList { get; private set; } = Array.Empty<string>();

    private static bool isBuilt = false;

    private static uint? lastWorldId = null;

    static DCWorlds()
    {
        Svc.Framework.Update += OnFrameworkUpdate;
        Svc.ClientState.Login += ClientState_Login;
    }

    private static void ClientState_Login()
    {
        isBuilt = false;
    }

    private static void OnFrameworkUpdate(IFramework framework)
    {
        if (Svc.ClientState.LocalPlayer == null) return;

        if (!isBuilt || Svc.ClientState.LocalPlayer.CurrentWorld.RowId != lastWorldId)
        {
            BuildWorlds();
            isBuilt = true;
            lastWorldId = Svc.ClientState.LocalPlayer.CurrentWorld.RowId;
        }
    }

    public static void BuildWorlds()
    {
        if (Svc.ClientState.LocalPlayer == null)
        {
            PluginLog.Warning("构建失败，获取不到角色信息");
            return;
        }
        
        DcId = null;
        DcName = "数据中心未初始化";
        WorldName = "世界未初始化";
        WorldsList = Array.Empty<string>();

        DcId = Svc.ClientState.LocalPlayer.CurrentWorld.Value.DataCenter.Value.RowId;
        DcName = Svc.ClientState.LocalPlayer.CurrentWorld.Value.DataCenter.Value.Name.ToString();
        WorldName = Svc.ClientState.LocalPlayer.CurrentWorld.Value.Name.ToString();
        WorldsList = Svc.Data.GetExcelSheet<World>()
            .Where(w => w.DataCenter.Value.RowId == DcId && w.RowId > 1000)
            .Select(w => w.Name.ToString())
            .Order()
            .ToArray();
    }
}
