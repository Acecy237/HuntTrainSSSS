using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using System.Collections.Generic;
namespace RankAHuntHelper.Tasks;

internal static class TaskMain
{
    public static TaskManager TaskManager = new();

    internal static bool IsRunning = false;

    internal static string stateString = string.Empty;
    internal static class TaskData
    {          
        internal static int TargetWorldIndex = -1;
        internal static string TargetWorld = string.Empty;

        internal static List<string> MapList= new();
        internal static int TargetMapIndex = -1;
        internal static string TargetMapName = string.Empty;

        internal static List<(float X, float Y, float Z)> FlyPointList = new();
        internal static int TargetFlyPointIndex = -1;
        internal static (float X, float Y, float Z) TargetFlyPoint => FlyPointList[TargetFlyPointIndex];

        internal static void Init()
        {            
            TargetWorldIndex = -1;
            TargetWorld = string.Empty;
            MapList.Clear();
            TargetMapIndex = -1;
            TargetMapName = string.Empty;
            FlyPointList.Clear();
            TargetFlyPointIndex = -1;
        }
    }

    internal static void StartTask()
    {
        if (IsRunning)
        {
            DuoLog.Error($"任务运行中");
            return;
        }
        
        ResetTask();

        IsRunning = true;

        Svc.Framework.Update += OnUpdate;
    }

    internal static void ResetTask()
    {
        IsRunning = false;
        TaskData.Init();
        Svc.Framework.Update -= OnUpdate;
    }

    internal static void OnUpdate(IFramework framework)
    {

    }

    internal static void ChangeStateString(string stateText) => stateString = stateText;
}
