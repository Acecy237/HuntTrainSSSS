using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.Automation;
using ImGuiNET;
using RankAHuntHelper.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static RankAHuntHelper.StaticData.ExpansionData;

namespace RankAHuntHelper.Windows;

public class MainWindow : Window, IDisposable
{    
    private bool enableNotice = false;
    private bool enableCrossWorld = false;
    private string selectedWorldName = string.Empty;
    private Dictionary<Expansion, bool> selectedExpansion = new();
    private Dictionary<Expansion, Dictionary<string, bool>> selectedMap = new();

    public MainWindow(RankAHuntHelper plugin)
        : base("RankAHuntHelper##aht", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        LoadConfig();
    }

    public void Dispose() 
    {
        
    }

    public override void Draw()
    {
        DrawInfoSection();
        DrawFunctionSection();
        if (enableCrossWorld)
        {            
            DrawCrossWorldSelector();
        }        
        DrawMapSelector();
        
        if (ImGui.Button("开始"))
        {            
            TaskMain.StartTask();
        }
        ImGui.SameLine();
        if (ImGui.Button("停止"))
        {            
            TaskMain.ResetTask();
        }
    }

    private static void DrawInfoSection()
    {
        ImGui.PushTextWrapPos();
        ImGui.TextUnformatted("我找怪，你来开\n依赖插件: LifeStream, Teleporter, Vnavmesh, HuntHelper");
        ImGui.PopTextWrapPos();
        ImGui.Separator();
        ImGui.Text($"插件状态: {(TaskMain.IsRunning ? "运行中" : "未运行")}");
        ImGui.Text($"当前任务: {TaskMain.stateString}"); 
        ImGui.Text($"当前地图: {TaskMain.TaskData.TargetMap} ({TaskMain.TaskData.TargetMapIndex + 1}/{TaskMain.TaskData.MapList.Count})");

        if (TaskMain.TaskData.MapList.Count > 0)
        {
            float progress = (TaskMain.TaskData.TargetMapIndex + 1) / (float)TaskMain.TaskData.MapList.Count;
            ImGui.ProgressBar(progress, new Vector2(300, 20), $"{TaskMain.TaskData.TargetMapIndex + 1} / {TaskMain.TaskData.MapList.Count}");
        }

        ImGui.Text($"当前点位: {TaskMain.TaskData.CurrentPointIndex + 1} / {TaskMain.TaskData.CurrentPointsList.Count}");

        if (TaskMain.TaskData.CurrentPointsList.Count > 0)
        {
            float progress = (TaskMain.TaskData.CurrentPointIndex + 1) / (float)TaskMain.TaskData.CurrentPointsList.Count;
            ImGui.ProgressBar(progress, new Vector2(300, 20), $"{TaskMain.TaskData.CurrentPointIndex + 1} / {TaskMain.TaskData.CurrentPointsList.Count}");
        }

        ImGui.Text($"当前地图 A 怪数量: {TaskMain.TaskData.EliteMarkCount}");
        ImGui.Text($"任务总共发现 A 怪: {TaskMain.TaskData.TotalEliteMarkCount}");

        ImGui.Separator();
    }

    private void DrawFunctionSection()
    {
        if (ImGui.Button("Hunt Helper"))
        {
            Svc.Commands.ProcessCommand("/hh");
            Svc.Commands.ProcessCommand("/hht");
        }
        ImGui.SameLine();
        ImGui.TextUnformatted("需手动录制(不支持自动开关捏)");
        if (ImGui.Checkbox("完成提示音", ref enableNotice)) SaveConfig();
        if (ImGui.Checkbox("自动跨服", ref enableCrossWorld)) SaveConfig();
    }

    private void DrawCrossWorldSelector()
    {
        var worldList = D.PlayerLocation.WorldsList;
        var currentIndex = Math.Max(0, worldList.IndexOf(selectedWorldName));
        using (var combo = ImRaii.Combo("##WorldCombo", string.IsNullOrEmpty(selectedWorldName) ? "请选择" : selectedWorldName))
        {
            if (combo)
            {
                for (int i = 0; i < worldList.Count; i++)
                {
                    var isSelected = worldList[i] == selectedWorldName;
                    if (ImGui.Selectable(worldList[i], isSelected))
                    {
                        selectedWorldName = worldList[i];
                        SaveConfig();
                    }
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
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
                        selectedMap[exp][map.MapName] = true;
                    }
                }

                ImGui.SameLine();

                var allSelected = value.Count > 0 && value.All(kvp => kvp.Value);

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

                        if (!selectedMap[exp].ContainsKey(map.MapName))
                            selectedMap[exp][map.MapName] = false;

                        var mapChecked = value[map.MapName];
                        if (ImGui.Checkbox(map.MapName, ref mapChecked))
                        {
                            selectedMap[exp][map.MapName] = mapChecked;
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
        RankAHuntHelper.Configuration.EnableNotice = enableNotice;
        RankAHuntHelper.Configuration.EnableCrossWorld = enableCrossWorld;
        RankAHuntHelper.Configuration.selectedWorldName = selectedWorldName;
        RankAHuntHelper.Configuration.SelectedExpansion = selectedExpansion;
        RankAHuntHelper.Configuration.SelectedMap = selectedMap;
        RankAHuntHelper.Configuration.Save();
    }

    private void LoadConfig()
    {
        enableNotice = RankAHuntHelper.Configuration.EnableNotice;
        enableCrossWorld = RankAHuntHelper.Configuration.EnableCrossWorld;
        selectedWorldName = RankAHuntHelper.Configuration.selectedWorldName;
        selectedExpansion = RankAHuntHelper.Configuration.SelectedExpansion
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        selectedMap = RankAHuntHelper.Configuration.SelectedMap
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new Dictionary<string, bool>(kvp.Value)
            );
    }
}
