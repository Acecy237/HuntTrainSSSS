using ECommons.GameHelpers;
using static RankAHuntHelper.TaskMain;

namespace RankAHuntHelper.Tasks;

internal static class TaskChangeWorlds
{    
    internal static void EnqueueChangeWorld(string worldName)
    {
        ChangeTaskState(TaskState.Teleport);
        T.ChangeStateString("跨服中...");
        T.TaskManager.Enqueue(() => S.Lifestream.TpChangeWorld(worldName));
        T.TaskManager.Enqueue(() => CheckWorldVisit(worldName));
    }

    private static bool CheckWorldVisit(string worldName)
    {
        if (Player.CurrentWorld == worldName && Player.Available && !Player.IsBusy)
        {
            T.ChangeStateString("跨服完成");
            TaskMain.ChangeTaskState(TaskMain.TaskState.ChangeMap);
            return true;
        }
        T.ChangeStateString("跨服中...");
        return false;
    }
}
