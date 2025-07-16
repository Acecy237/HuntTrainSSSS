using ECommons.EzIpcManager;
using ECommons.Logging;
using RankAHuntTrainAssistant;
using System;

namespace RankAHuntTrainAssistant.Services;

public class LifestreamIPC
{
    [EzIPC] public Func<bool>? CanChangeInstance;
    [EzIPC] public Func<int>? GetNumberOfInstances;
    [EzIPC] public Func<int>? GetCurrentInstance;
    [EzIPC] public Action<int>? ChangeInstance;
    [EzIPC] public Action<string, bool, string, bool, int?, bool?, bool?>? TPAndChangeWorld;


    public LifestreamIPC()
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

    public int GetCurrentInstanceIndex()
    {
        try
        {
            return GetCurrentInstance?.Invoke() ?? 0;
        }
        catch (Exception ex)
        {
            PluginLog.Error("Error getting current instance index: " + ex);
            return 0;
        }
    }

    public void ChangeWorld(string world)
    {
        TPAndChangeWorld?.Invoke(
            world,
            false,
            string.Empty,
            true,
            null,
            true,
            true
            );
    }

}
