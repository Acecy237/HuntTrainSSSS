using RankAHuntHelper.Services;

namespace RankAHuntHelper;

public static class ServiceManager
{
    public static LifestreamIPC Lifestream = null!;
    public static TeleporterIPC Teleporter = null!;
    public static VnavmeshIPC Vnavmesh = null!;
    public static HuntHelperIPC HuntHelper = null!;
}
