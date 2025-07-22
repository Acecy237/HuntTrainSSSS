using ECommons.EzIpcManager;
using RankAHuntHelper.StaticData;
using System.Linq;
using System;

namespace RankAHuntHelper.Services;

public class TeleporterIPC
{
    [EzIPC(applyPrefix: false)] public Func<uint, byte, bool>? Teleport;

    private TeleporterIPC()
    {
        EzIPC.Init(this, "Teleport", SafeWrapper.AnyException);
    }

    public bool AetheryteTp(uint aetheryteId) => Teleport?.Invoke(aetheryteId, 0) ?? false;
    public bool MapTeleport(string mapName)
    {
        foreach (var mapList in MapData.ExpansionToMapData.Values)
        {
            var map = mapList.FirstOrDefault(m => m.MapName == mapName);
            if (map != null)
            {
                return AetheryteTp(map.AetheryteId);
            }
        }
        return false;
    }
}
