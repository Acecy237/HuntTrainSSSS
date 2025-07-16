using ECommons.EzIpcManager;
using ECommons.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RankAHuntTrainAssistant.Services;

public class HuntHelperIPC
{
    [EzIPC] public Func<List<object>>? GetTrainList;
    [EzIPC] public Func<uint>? GetVersion;

    public HuntHelperIPC()
    {
        EzIPC.Init(this, "HuntHelper", SafeWrapper.AnyException);
    }

    public List<HHMobRecord> TryGetMobList()
    {
        try
        {
            if (GetTrainList == null || GetVersion == null)
            {
                PluginLog.Warning("[HuntHelperIPC] IPC 尚未就绪");
                return new();
            }

            if (GetVersion.Invoke() < 1)
            {
                PluginLog.Warning("[HuntHelperIPC] IPC 版本不兼容");
                return new();
            }

            var rawList = GetTrainList.Invoke();
            return rawList.Select(obj =>
            {
                dynamic d = obj;
                return new HHMobRecord(
                    d.Name, d.MobID, d.TerritoryID, d.MapID, d.Instance,
                    d.Position, d.Dead, d.LastSeenUTC
                );
            }).ToList();
        }
        catch (Exception ex)
        {
            PluginLog.Error("[HuntHelperIPC] 获取列车失败: " + ex.Message);
            return new();
        }
    }

    public record struct HHMobRecord(
        string Name,
        uint MobID,
        uint TerritoryID,
        uint MapID,
        uint Instance,
        Vector2 Position,
        bool Dead,
        DateTime LastSeenUTC
    );
}
