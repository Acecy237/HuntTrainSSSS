using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons;
using ECommons.Automation;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.Logging;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Lumina.Excel.Sheets;
using RankAHuntTrainAssistant.Manager;
using RankAHuntTrainAssistant.Services;
using RankAHuntTrainAssistant.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static FFXIVClientStructs.FFXIV.Client.Network.NetworkModuleProxy.Delegates;

namespace RankAHuntTrainAssistant.Windows;

public class MainWindow : Window, IDisposable
{
    private bool enableCrossWorld = false;

    public MainWindow(Plugin plugin)
        : base("RankAHuntTrainAssistant##aht", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        LoadFromConfig();
    }

    public void Dispose() {
    
    }

    private void LoadFromConfig()
    {      

    }

    private void SaveConfig()
    {
        Plugin.Configuration.Save();
    }

    public override void Draw()
    {
        DrawInfoSection();
        DrawFunctionSection();
        if (enableCrossWorld)
        {
            DrawCrossWorldSelector();
        }
        DrawVersionSelector();
        if (ImGui.Button("功能测试"))
        {
            Svc.Chat.Print("WorldId: " + Svc.ClientState.LocalPlayer?.CurrentWorld.RowId);
        }
    }

    private void DrawInfoSection()
    {
        ImGui.PushTextWrapPos();
        ImGui.TextUnformatted("自动寻路找A怪\n依赖插件: Vnavmesh、Lifestream");
        ImGui.PopTextWrapPos();
        ImGui.Separator();
        ImGui.TextUnformatted($"角色当前状态");
        ImGui.TextUnformatted($"数据中心: {MapManager.DcName}");
        ImGui.TextUnformatted($"服务器: {MapManager.WorldName}");
        ImGui.TextUnformatted($"地图: {MapManager.MapName}");
        ImGui.TextUnformatted($"地图ID: {MapManager.MapId}");
        ImGui.TextUnformatted($"分线: {MapManager.InstanceName}");
        ImGui.Separator();
    }

    private void DrawFunctionSection()
    {        
        ImGui.Text("功能设置");

        if (ImGui.Button("Hunt Helper"))
        {
            Svc.Commands.ProcessCommand("/hh");
            Svc.Commands.ProcessCommand("/hht");
        }

        ImGui.SameLine();
        ImGui.TextUnformatted("需手动开始录制");

        if (ImGui.Checkbox("跨服功能", ref enableCrossWorld))
        {
            SaveConfig();
        }
    }

    private void DrawCrossWorldSelector()
    {
        ImGui.TextUnformatted("选择服务器:");
        if (ImGui.BeginTable("WorldTable", 3))
        {

        }
        
    }

    private void DrawVersionSelector()
    {

    }

}
