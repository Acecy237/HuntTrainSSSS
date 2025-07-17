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
using static RankAHuntTrainAssistant.StaticData.ExpansionData;

namespace RankAHuntTrainAssistant.Windows;

public class MainWindow : Window, IDisposable
{
    private bool enableCrossWorld = false;
    private Dictionary<string, bool> selectedWorld = new();
    private Dictionary<Expansion, bool> selectedExpansion = new();
    private Dictionary<Expansion, Dictionary<string, bool>> selectedMap = new();

    public MainWindow(Plugin plugin)
        : base("RankAHuntTrainAssistant##aht", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        LoadConfig();
    }

    public void Dispose() {
    
    }

    public override void Draw()
    {
        DrawInfoSection();
        DrawFunctionSection();
        if (enableCrossWorld && MapManager.WorldsList != null)
        {
            DrawCrossWorldSelector();
        }
        DrawMapSelector();
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

        bool allSelected = MapManager.WorldsList.All(w => selectedWorld.ContainsKey(w) && selectedWorld[w]);

        ImGui.SameLine();
        if (ImGui.SmallButton(allSelected ? "取消全选" : "全选"))
        {
            foreach (var world in MapManager.WorldsList)
            {
                selectedWorld[world] = !allSelected;
            }
            SaveConfig();
        }

        using var table = ImRaii.Table("WorldTable", 3);
        if (table)
        {
            foreach (var world in MapManager.WorldsList)
            {
                ImGui.TableNextColumn();

                if (!selectedWorld.ContainsKey(world))
                    selectedWorld[world] = false;

                bool isChecked = selectedWorld[world];
                if (ImGui.Checkbox(world, ref isChecked))
                {
                    selectedWorld[world] = isChecked;
                    SaveConfig();
                }
            }
        }
    }

    private void DrawMapSelector()
    {
        ImGui.TextUnformatted("选择版本:");
        ImGui.Spacing();

        foreach (Expansion exp in Enum.GetValues(typeof(Expansion)))
        {
            if (!selectedExpansion.TryGetValue(exp, out var isChecked))
            {
                isChecked = false;
                selectedExpansion[exp] = false;
            }

            if (ImGui.Checkbox(exp.ToString(), ref isChecked))
            {
                selectedExpansion[exp] = isChecked;
                SaveConfig();
            }

            if (selectedExpansion[exp])
            {
                var mapList = MapData.ExpansionToMapData.TryGetValue(exp, out var list) ? list : null;
                if (mapList == null) continue;

                if (!selectedMap.TryGetValue(exp, out var value))
                {
                    value = new();
                    selectedMap[exp] = value;
                    foreach (var map in mapList)
                    {
                        selectedMap[exp][map.Name] = true;
                    }
                }

                ImGui.SameLine();

                bool allSelected = value.Count > 0 && value.All(kvp => kvp.Value);

                if (ImGui.SmallButton(allSelected ? $"取消全选##{exp}" : $"全选##{exp}"))
                {
                    foreach (var mapName in value.Keys.ToList())
                    {
                        value[mapName] = !allSelected;
                    }
                    SaveConfig();
                }

                ImGui.Indent();
                ImGui.Spacing();

                if (ImGui.BeginTable($"MapTable_{exp}", 3))
                {
                    foreach (var map in mapList)
                    {
                        ImGui.TableNextColumn();

                        if (!selectedMap[exp].ContainsKey(map.Name))
                            selectedMap[exp][map.Name] = false;

                        bool mapChecked = value[map.Name];
                        if (ImGui.Checkbox(map.Name, ref mapChecked))
                        {
                            selectedMap[exp][map.Name] = mapChecked;
                            SaveConfig();
                        }
                    }
                    ImGui.EndTable();
                }
                ImGui.Unindent();
            }
        }
    }

    private void SaveConfig()
    {
        Plugin.Configuration.EnableCrossWorld = enableCrossWorld;
        Plugin.Configuration.SelectedWorlds = selectedWorld;
        Plugin.Configuration.SelectedExpansion = selectedExpansion;
        Plugin.Configuration.SelectedMap = selectedMap;
        Plugin.Configuration.Save();
    }

    private void LoadConfig()
    {
        enableCrossWorld = Plugin.Configuration.EnableCrossWorld;

        selectedWorld = Plugin.Configuration.SelectedWorlds
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        selectedExpansion = Plugin.Configuration.SelectedExpansion
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        selectedMap = Plugin.Configuration.SelectedMap
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new Dictionary<string, bool>(kvp.Value)
            );
    }
}
