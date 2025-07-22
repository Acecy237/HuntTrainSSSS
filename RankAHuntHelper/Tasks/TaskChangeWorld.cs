using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommons;
using ECommons.GameHelpers;

namespace RankAHuntHelper.Tasks;

internal static class TaskChangeWorlds
{
    internal static bool EnqueueChangeWorld(string worldName)
    {
        TaskMain.TaskManager.Enqueue(() => S.Lifestream.TpChangeWorld(worldName));
        TaskMain.TaskManager.Enqueue(() => CheckWorldVisit(worldName));
        return true;
    }   

    private static bool CheckWorldVisit(string worldName)
    {
        if (Player.CurrentWorld == worldName && Player.Available) return true;
        return false;
    }
}
