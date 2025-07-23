using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation;
using ECommons.Automation.NeoTaskManager;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Lumina.Data.Parsing.Layer;
using RankAHuntHelper.StaticData;
using RankAHuntHelper.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static FFXIVClientStructs.FFXIV.Client.Network.NetworkModuleProxy.Delegates;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonArmouryBoard.Delegates;
using static RankAHuntHelper.StaticData.ExpansionData;

namespace RankAHuntHelper;

internal static class TaskMain
{
    public static TaskManager TaskManager = new();

    internal static bool IsRunning = false;

    internal static string stateString = string.Empty;
    internal static class TaskData
    {
        internal static string TargetWorld = string.Empty;

        internal static List<string> MapList = new();
        internal static int TargetMapIndex = -1;
        internal static string TargetMap = string.Empty;
        internal static ushort TargetMapId = 0;

        internal static bool IsMapInstatnce = false;
        internal static int TargetInstance = -1;
        internal static int CurrentInstance = -1;
        internal static int TotalInstance = -1;
        internal static bool IsBuiltInstance = false;

        internal static int CurrentPointIndex = 0;
        internal static List<(float X, float Y, float Z)> CurrentPointsList = new();

        internal static Vector3? LastFlyTarget = null;
        internal static int EliteMarkCount = 0;
        internal static int TotalEliteMarkCount = 0;
        internal static HashSet<uint> CurrentEliteMarks = new();

        internal static void Init()
        {
            TargetWorld = string.Empty;
            MapList.Clear();
            TargetMapIndex = -1;
            TargetMap = string.Empty;
            TargetMapId = 0;
            IsMapInstatnce = false;
            TargetInstance = -1;
            CurrentInstance = -1;
            TotalInstance = -1;
            IsBuiltInstance = false;
            CurrentPointIndex = 0;
            CurrentPointsList = new();
            LastFlyTarget = null;
            EliteMarkCount = 0;
            TotalEliteMarkCount = 0;
            CurrentEliteMarks = new();
        }

        internal static bool GetQueue()
        {
            var crossWorldEnabled = RankAHuntHelper.Configuration.EnableCrossWorld;
            var selectedWorldName = RankAHuntHelper.Configuration.selectedWorldName;
            var selectedExpansion = RankAHuntHelper.Configuration.SelectedExpansion;
            var selectedMap = RankAHuntHelper.Configuration.SelectedMap;

            if (crossWorldEnabled)
            {
                TargetWorld = selectedWorldName;
            }
            else
            {
                TargetWorld = Player.CurrentWorld;
            }

            foreach (var exp in Enum.GetValues<Expansion>())
            {
                if (!selectedExpansion.TryGetValue(exp, out var expEnabled) || !expEnabled) continue;

                if (!selectedMap.TryGetValue(exp, out var mapDict)) continue;
                if (!MapData.ExpansionToMapData.TryGetValue(exp, out var mapList)) continue;

                foreach (var map in mapList)
                {
                    if (mapDict.TryGetValue(map.MapName, out var isChecked) && isChecked)
                    {
                        MapList.Add(map.MapName);
                    }
                }
            }
            if (MapList.Count == 0)
            {
                Svc.Chat.PrintError("没有选择地图");
                ResetTask();
                return false;
            }

            if (RankAHuntHelper.Configuration.EnableCrossWorld)
            {
                if (string.IsNullOrEmpty(TargetWorld))
                {
                    Svc.Chat.PrintError("没有选择世界");
                    ResetTask();
                    return false;
                }
            }
            else
            {
                TargetWorld = Player.CurrentWorld;
            }
            return true;
        }

        internal static bool GetNextMap()
        {
            TargetMapIndex++;
            if (TargetMapIndex >= MapList.Count)
            {
                return false;
            }
            TargetMap = MapList[TargetMapIndex];
            TargetMapId = GetMapId(TargetMap);
            return true;
        }
        internal static ushort GetMapId(string mapName)
        {
            foreach (var (_, mapList) in MapData.ExpansionToMapData)
            {
                var map = mapList.FirstOrDefault(m => m.MapName == mapName);
                if (map != null)
                {
                    return map.MapId;
                }
            }
            return 0;
        }

        internal static void BuildInstance()
        {
            CurrentInstance = S.Lifestream.GetCurrentInstance?.Invoke() ?? 0;
            TotalInstance = S.Lifestream.GetNumberOfInstances?.Invoke() ?? 0;
            IsMapInstatnce = TotalInstance > 0;
            if (IsMapInstatnce) TargetInstance = 0;
            IsBuiltInstance = true;
        }

        internal static void GetInstance()
        {
            CurrentInstance = S.Lifestream.GetCurrentInstance?.Invoke() ?? 0;
        }

        internal static bool GetNextInstance()
        {
            TargetInstance ++;
            if (TargetInstance > TotalInstance)
            {
                return false;
            }
            return true;
        }
    }

    internal enum TaskState
    {
        Idle,
        PrepareQueue,
        ChangeWorld,
        ChangeMap,
        ChangeInstance,
        Teleport,
        PrepareFly,
        Flying,
        EndTask,
    }

    internal static TaskState State = TaskState.Idle;

    internal static void ChangeStateString(string stateText) => stateString = stateText;

    internal static void ChangeTaskState(TaskState state) => State = state;

    internal static void StartTask()
    {
        if (IsRunning)
        {
            Svc.Chat.PrintError("任务已经在运行中");
            return;
        }
        if (Player.IsBusy)
        {
            Svc.Chat.PrintError("角色忙");
            return;
        }

        ResetTask();
        Svc.Framework.Update += OnUpdate;
        IsRunning = true;
        ChangeTaskState(TaskState.PrepareQueue);
    }

    internal static void ResetTask()
    {
        Svc.Framework.Update -= OnUpdate;
        VnavmeshStop();
        ChangeTaskState(TaskState.Idle);
        IsRunning = false;
        TaskData.Init();
        TaskManager.Abort();
    }

    private static void OnUpdate(IFramework framework)
    {
        switch (State)
        {
            case TaskState.Idle:
                break;
            case TaskState.PrepareQueue:
                PrepareQueue();
                break;
            case TaskState.ChangeWorld:
                ChangeWorld();
                break;
            case TaskState.ChangeMap:
                ChangeMap();
                break;
            case TaskState.ChangeInstance:
                ChangeInstance();
                break;
            case TaskState.Teleport:
                break;
            case TaskState.PrepareFly:
                PrepareFly();
                break;
            case TaskState.Flying:
                Flying();
                break;
            case TaskState.EndTask:
                ResetTask();
                ChangeTaskState(TaskState.Idle);
                break;

        }
    }

    private static void PrepareQueue()
    {
        TaskData.GetQueue();
        ChangeTaskState(TaskState.ChangeWorld);
    }

    private static void ChangeWorld()
    {
        if (!RankAHuntHelper.Configuration.EnableCrossWorld)
        {
            ChangeTaskState(TaskState.ChangeMap);
            return;
        }              
        if (Player.CurrentWorld == TaskData.TargetWorld)
        {
            ChangeTaskState(TaskState.ChangeMap);
            return;
        }
        TaskChangeWorlds.EnqueueChangeWorld(TaskData.TargetWorld);
    }

    private static void ChangeMap()
    {
        if (TaskData.IsMapInstatnce)
        {
            if ((TaskData.TargetInstance + 1) > TaskData.TotalInstance)
            {
                ChangeTaskState(TaskState.ChangeInstance);
                return;
            }
            TaskChangeMaps.EnqueueChangeMap(TaskData.TargetMap, TaskData.TargetMapId);
            return;
        }
        if (!TaskData.GetNextMap())
        {
            ChangeTaskState(TaskState.EndTask);
            return;
        } 
        if (Player.Territory == TaskData.TargetMapId)
        {
            ChangeTaskState(TaskState.ChangeInstance);
            return;
        }
        TaskChangeMaps.EnqueueChangeMap(TaskData.TargetMap, TaskData.TargetMapId);
    }

    internal static void ChangeInstance()
    {
        if (!TaskData.IsBuiltInstance)
        {
            TaskData.BuildInstance();
        }
        if (!TaskData.IsMapInstatnce)
        {
            TaskData.IsBuiltInstance = false;
            ChangeTaskState(TaskState.PrepareFly);
            return;
        }
        if (!TaskData.GetNextInstance())
        {
            TaskData.IsMapInstatnce = false;
            TaskData.IsBuiltInstance = false;
            ChangeTaskState(TaskState.ChangeMap);
            return;
        }

        if (TaskData.CurrentInstance == TaskData.TargetInstance)
        {
            ChangeTaskState(TaskState.PrepareFly);
            return;
        }
        TaskChangeInstance.EnqueueChangeInstance(TaskData.TargetInstance);
    }

    internal unsafe static void PrepareFly()
    {
        if (Player.IsBusy) return;
        TaskData.GetInstance();
        if (!Player.Mounted)
        {
            ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9);
        }        
        TaskData.CurrentEliteMarks.Clear();
        TaskData.EliteMarkCount = 0;
        TaskData.CurrentPointIndex = 0;
        TaskData.CurrentPointsList = GetRankASpawnsByMapName(D.PlayerLocation.MapName); 
        TaskData.CurrentPointsList = PathHelper.GetGreedy2OptPath(TaskData.CurrentPointsList, Player.Position);
        ChangeTaskState(TaskState.Flying);
    }

    internal unsafe static void Flying()
    {        
        if (TaskData.CurrentPointIndex >= TaskData.CurrentPointsList.Count || TaskData.EliteMarkCount >= 2)
        {
            VnavmeshStop();
            ChangeTaskState(TaskState.ChangeMap);
            return;
        }
        var targetPoint = TaskData.CurrentPointsList[TaskData.CurrentPointIndex];
        var target = new Vector3(targetPoint.X, targetPoint.Y, targetPoint.Z);
        var distance = Player.DistanceTo(target);
        if (distance < 70f || CheckAndUpdateEliteMark())
        {
            TaskData.LastFlyTarget = null;
            TaskData.CurrentPointIndex ++;
            return;
        }
        if(!Player.IsMoving)
        {
            Flyto(target.X, target.Y, target.Z);
            TaskData.LastFlyTarget = target;
        }
        if (TaskData.LastFlyTarget == null)
        {
            Flyto(target.X, target.Y, target.Z);
            TaskData.LastFlyTarget = target;
            ChangeStateString("飞行到下一个点");            
        }
    }

    public static void Flyto(float x, float y, float z)
    {
        Chat.ExecuteCommand($"/vnav flyto {x} {y} {z}");
    }

    public static void VnavmeshStop()
    {
        Chat.ExecuteCommand("/vnav stop");
    }

    public static List<(float X, float Y, float Z)> GetRankASpawnsByMapName(string mapName)
    {
        foreach (var expansionEntry in MapData.ExpansionToMapData)
        {
            foreach (var map in expansionEntry.Value)
            {
                if (map.MapName == mapName)
                {
                    return map.RankASpawns;
                }
            }
        }
        return new();
    }

    public static bool CheckAndUpdateEliteMark()
    {
        foreach (IBattleChara battle in Svc.Objects.OfType<IBattleChara>())
        {
            float distance = Player.DistanceTo(battle.Position);

            uint nameId = battle.NameId;

            if (!MobsData.mobsNameId.Contains(nameId))
                continue;

            if (TaskData.CurrentEliteMarks.Contains(nameId))
                continue;

            TaskData.CurrentEliteMarks.Add(nameId);
            TaskData.EliteMarkCount++;
            TaskData.TotalEliteMarkCount++;

            Svc.Chat.Print($"发现第 {TaskData.TotalEliteMarkCount} 只 A 怪：{battle.Name.TextValue}");

            return true;
        }
        return false;
    }

    public static class PathHelper
    {
        public static List<(float X, float Y, float Z)> GetGreedy2OptPath(
            List<(float X, float Y, float Z)> points,
            Vector3 currentPos)
        {
            var path = GetGreedyPath(points, currentPos);

            path = Apply2Opt(path);

            return path;
        }

        private static List<(float X, float Y, float Z)> GetGreedyPath(
            List<(float X, float Y, float Z)> points,
            Vector3 currentPos)
        {
            var remaining = new List<(float X, float Y, float Z)>(points);
            var path = new List<(float X, float Y, float Z)>();

            var current = (currentPos.X, currentPos.Y, currentPos.Z);
            while (remaining.Count > 0)
            {
                var next = remaining.OrderBy(p => Distance(current, p)).First();
                path.Add(next);
                remaining.Remove(next);
                current = next;
            }

            return path;
        }

        private static List<(float X, float Y, float Z)> Apply2Opt(List<(float X, float Y, float Z)> path)
        {
            bool improved = true;
            while (improved)
            {
                improved = false;
                for (int i = 1; i < path.Count - 2; i++)
                {
                    for (int j = i + 1; j < path.Count - 1; j++)
                    {
                        double d1 = Distance(path[i - 1], path[i]) + Distance(path[j], path[j + 1]);
                        double d2 = Distance(path[i - 1], path[j]) + Distance(path[i], path[j + 1]);

                        if (d2 < d1)
                        {
                            path.Reverse(i, j - i + 1);
                            improved = true;
                        }
                    }
                }
            }
            return path;
        }

        public static double Distance((float X, float Y, float Z) a, (float X, float Y, float Z) b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            var dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
