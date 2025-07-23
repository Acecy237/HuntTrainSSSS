using ECommons.EzIpcManager;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace RankAHuntHelper.Services;

public class VnavmeshIPC
{
    [EzIPC("Nav.IsReady")] public readonly Func<bool> IsReady = () => false;
    [EzIPC("Path.MoveTo")] public readonly Action<List<Vector3>, bool> MoveTo = (path, flag) => { };
    [EzIPC("Path.Stop")] public readonly Action Stop = () => { };

    private VnavmeshIPC()
    {
        EzIPC.Init(this, "vnavmesh", SafeWrapper.AnyException);
    }
}
