using ECommons.EzIpcManager;
using ECommons.GameHelpers;
using System;

namespace RankAHuntHelper.Services;

public class LifestreamIPC
{
    [EzIPC] public Func<bool> IsBusy { get; private set; } = () => false;
    [EzIPC] public Func<bool> CanChangeInstance { get; private set; } = () => false;
    [EzIPC] public Func<int> GetNumberOfInstances { get; private set; } = () => 1;
    [EzIPC] public Func<int> GetCurrentInstance { get; private set; } = () => 0;
    [EzIPC] public Func<string, bool> ChangeWorld { get; private set; } = _ => false;
    [EzIPC] public Action<int> ChangeInstance { get; private set; } = _ => { };
    [EzIPC] public Action? Abort { get; private set; } = null;

    private LifestreamIPC()
    {
        EzIPC.Init(this, "Lifestream", SafeWrapper.AnyException);
    }

    public void TrySwitchToNextInstance()
    {
        if (CanChangeInstance?.Invoke() != true) return;

        int current = GetCurrentInstance?.Invoke() ?? 0;
        int total = GetNumberOfInstances?.Invoke() ?? 1;

        if (total <= 1)
        {
            ChangeInstance?.Invoke(0);
            return;
        }

        int next = current + 1;
        if (next > total) next = 1;

        ChangeInstance?.Invoke(next);
    }

    public int GetCurrentInstanceIndex() => GetCurrentInstance?.Invoke() ?? 0;

    public bool TpChangeWorld(string world)
    {
        if (Player.IsBusy)
        {
            Svc.Chat.PrintError("角色忙，无法传送");
            return false;
        }
        if (IsBusy?.Invoke() ?? false)
        {
            Svc.Chat.PrintError("Lifestream传送中，无法传送");
            return false;
        }
        return ChangeWorld?.Invoke(world) ?? false;
    }
}
