using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Lumina.Excel.Sheets;
using System;
using System.Numerics;

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
        ImGui.TextUnformatted("自动寻路找A怪\n依赖插件: Vnavmesh、Lifesteam");
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        if (ImGui.Button("测试地图ID"))
        {
            currentMapId = Svc.ClientState.TerritoryType;
            Svc.Chat.Print($"当前地图ID: {currentMapId}");

            unsafe
            {
                var instanceId = UIState.Instance()->PublicInstance.InstanceId;
                Svc.Chat.Print($"当前地图分线: {instanceId}");
            }


        }
    }
}

// 
