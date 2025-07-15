using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace RankAHuntTrainAssistant;

public class MainWindow : Window, IDisposable
{

    // 状态变量
    private ushort currentMapId = 0;

    public MainWindow(Plugin plugin)
        : base("RankAHuntTrainAssistant##aht", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.PushTextWrapPos();
        ImGui.TextUnformatted("功能:自动寻路找A怪\n依赖插件: Vnavmesh、Teleporter");
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        if (ImGui.Button("测试地图ID"))
        {
            currentMapId = Svc.ClientState.TerritoryType;

            Svc.Chat.Print($"当前地图ID: {currentMapId}");
        }
    }
}
