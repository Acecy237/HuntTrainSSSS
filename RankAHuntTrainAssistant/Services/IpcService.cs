using RankAHuntTrainAssistant.Services;

namespace RankAHuntTrainAssistant;

public static class IpcService
{
    public static LifestreamIPC Lifestream { get; private set; } = null!;
    public static VnavmeshIPC Vnavmesh { get; private set; } = null!;
    public static HuntHelperIPC HuntHelper { get; private set; } = null!;

    public static void InitAll()
    {
        Lifestream = new LifestreamIPC();
        Vnavmesh = new VnavmeshIPC();
        HuntHelper = new HuntHelperIPC();
    }
}
