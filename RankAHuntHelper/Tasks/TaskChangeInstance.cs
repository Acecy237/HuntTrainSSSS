using ECommons.Automation;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using System.Linq;
using System.Numerics;
using static ECommons.GenericHelpers;
using static RankAHuntHelper.TaskMain;

namespace RankAHuntHelper.Tasks;
public static class TaskChangeInstance
{
    public static void EnqueueChangeInstance(Number num)
    {
        T.ChangeTaskState(TaskState.Teleport);
        T.ChangeStateString("切换分线中");
        T.TaskManager.Enqueue(() => Player.Interactable);
        T.TaskManager.Enqueue(() =>
        {
            if (S.Lifestream.GetNumberOfInstances() == 0 || num == 0 || S.Lifestream.GetCurrentInstance() == num) return null;
            return true;
        });
        T.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
        T.TaskManager.Enqueue(() =>
        {
            if (!S.Lifestream.CanChangeInstance())
            {
                var nearestAetheryte = Svc.Objects.Where(x => x.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Aetheryte && x.IsTargetable).OrderBy(x => Vector3.Distance(Player.Position, x.Position)).FirstOrDefault();
                if (nearestAetheryte != null)
                {
                    if (nearestAetheryte.IsTarget() && EzThrottler.Throttle("Lockon"))
                    {
                        Chat.ExecuteCommand("/lockon");
                        T.TaskManager.Insert(() => Chat.ExecuteCommand("/automove on"));
                        return true;
                    }
                    else
                    {
                        if (EzThrottler.Throttle("SetTarget"))
                        {
                            Svc.Targets.Target = nearestAetheryte;
                        }
                        return false;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return true;
            }
        });
        T.TaskManager.Enqueue(() =>
        {
            if (!Player.Mounted)Chat.ExecuteCommand("/ac 随机坐骑");
        });
        T.TaskManager.EnqueueDelay(2000);
        T.TaskManager.Enqueue(() =>Chat.ExecuteCommand("/gaction 跳跃"));
        T.TaskManager.Enqueue(() =>
        {
            if (S.Lifestream.GetCurrentInstance() == num) return true;
            if (S.Lifestream.CanChangeInstance())
            {
                Chat.ExecuteCommand("/automove off");
                S.Lifestream.ChangeInstance(num);
                return true;
            }
            return false;
        }, new(timeLimitMS: 15000));
        T.TaskManager.EnqueueDelay(4000);
        T.TaskManager.Enqueue(() =>
        {
            if (Player.Available && !Player.IsBusy)
            {
                T.ChangeStateString($"切换分线完成");
                T.ChangeTaskState(T.TaskState.PrepareFly);
                return true;
            }
            T.ChangeStateString("切换分线中");
            return false;
        });
    }
}
