using ECommons.EzIpcManager;
using System;

namespace RankAHuntHelper.Services;

public class VnavmeshIPC
{
    [EzIPC("Nav.IsReady")] public readonly Func<bool>? IsReady;

    private VnavmeshIPC()
    {
        EzIPC.Init(this, "vnavmesh", SafeWrapper.AnyException);
    }

    public bool IsVnavmeshReady()
    {
        try
        {
            return IsReady?.Invoke() ?? false;
        }
        catch (Exception ex)
        {
            PluginLog.Error("Error checking Vnavmesh readiness: "+ ex);
            return false;
        }
    }
}
