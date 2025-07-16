using ECommons;
using ECommons.EzIpcManager;
using ECommons.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RankAHuntTrainAssistant.Services;

public class VnavmeshIPC
{
    [EzIPC("Nav.IsReady")] public readonly Func<bool>? IsReady;

    public VnavmeshIPC()
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
