using ECommons.EzIpcManager;
using System;

namespace RankAHuntHelper.Services;

public class TeleporterIPC
{
    [EzIPC(applyPrefix: false)] public Func<uint, byte, bool>? Teleport;

    private TeleporterIPC()
    {
        EzIPC.Init(this, "Teleport", SafeWrapper.AnyException);
    }

    public bool TpAetheryte(uint aetheryteId) => Teleport?.Invoke(aetheryteId, 0) ?? false;
}
