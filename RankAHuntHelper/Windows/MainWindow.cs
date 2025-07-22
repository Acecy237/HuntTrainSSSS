using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using RankAHuntHelper.StaticData;
using RankAHuntHelper.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using static RankAHuntHelper.StaticData.ExpansionData;

namespace RankAHuntHelper.Windows;

public class MainWindow : Window, IDisposable
{    
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
        ImGui.TextUnformatted("绝伊甸自动加低保\n依赖插件: AEassist, SplatoonX, Nyadraw, kodakkuAssist");
        ImGui.PopTextWrapPos();
        ImGui.Separator();
        ImGui.TextUnformatted($"角色当前状态");
        ImGui.TextUnformatted($"数据中心: {D.PlayerLocation.DcName}");
        ImGui.TextUnformatted($"服务器: {D.PlayerLocation.WorldName}");
        ImGui.TextUnformatted($"地图: {D.PlayerLocation.MapName}");
        ImGui.TextUnformatted($"地图ID: {D.PlayerLocation.MapId}");
        ImGui.TextUnformatted($"分线: {D.PlayerLocation.InstanceName}");
        ImGui.Separator();
        ImGui.Text($"插件状态: {(TaskMain.IsRunning ? "运行中" : "未运行")}");
        ImGui.Text($"当前任务: {TaskMain.stateString}");
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
        ImGui.TextUnformatted("需手动录制(它没有支持调用开关捏)");
        if (ImGui.Checkbox("自动跨服(勾选必须选择服务器）", ref enableCrossWorld)) SaveConfig();
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
        RankAHuntHelper.Configuration.EnableCrossWorld = enableCrossWorld;
        RankAHuntHelper.Configuration.selectedWorldName = selectedWorldName;
        RankAHuntHelper.Configuration.SelectedExpansion = selectedExpansion;
        RankAHuntHelper.Configuration.SelectedMap = selectedMap;
        RankAHuntHelper.Configuration.Save();
    }

    private void LoadConfig()
    {
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
