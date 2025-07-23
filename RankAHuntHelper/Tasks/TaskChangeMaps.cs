using ECommons.GameHelpers;
using RankAHuntHelper.StaticData;
using System.Linq;
using static RankAHuntHelper.TaskMain;

namespace RankAHuntHelper.Tasks;

internal static class TaskChangeMaps
{
    internal static void EnqueueChangeMap(string mapName, ushort mapId)
    {
        ChangeTaskState(TaskState.Teleport);
        T.ChangeStateString("传送中...");
        T.TaskManager.Enqueue(() => S.Teleporter.MapTeleport(mapName));
        T.TaskManager.EnqueueDelay(3000);
        T.TaskManager.Enqueue(() => CheckMapVisit(mapName, mapId));
    }

    private static bool CheckMapVisit(string mapName, ushort mapId)
    {        
        if (Player.Territory == mapId && Player.Available && !Player.IsBusy)
        {
            T.ChangeStateString($"到达{mapName}");
            T.ChangeTaskState(T.TaskState.ChangeInstance);
            return true;
        }
        T.ChangeStateString($"传送{mapName}");
        return false;
    }
}
