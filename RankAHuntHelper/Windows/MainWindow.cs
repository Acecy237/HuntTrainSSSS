using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using RankAHuntHelper.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using static RankAHuntHelper.StaticData.ExpansionData;

namespace RankAHuntHelper.Windows;

public class MainWindow : Window, IDisposable
{
    private bool enableCrossWorld = false;
    private Dictionary<string, bool> selectedWorld = new();
    private Dictionary<Expansion, bool> selectedExpansion = new();
    private Dictionary<Expansion, Dictionary<string, bool>> selectedMap = new();

    public MainWindow(RankAHuntHelper plugin)
        : base("RankAHuntHelper##aht", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        LoadConfig();
    }

    public void Dispose() {
    
    }

    public override void Draw()
    {
        DrawInfoSection();
        DrawFunctionSection();
        if (enableCrossWorld && DataManager.PlayerLocation.WorldsList != null)
        {
            DrawCrossWorldSelector();
        }
        DrawMapSelector();

        if (ImGui.Button("开始"))
        {

        }

        ImGui.SameLine();

        if (ImGui.Button("暂停"))
        {

        }

        ImGui.SameLine();

        if (ImGui.Button("停止"))
        {

        }

        if (ImGui.Button("功能测试"))
        {
            Svc.Chat.Print("---------测试---------");
        }
    }

    private static void DrawInfoSection()
    {
        ImGui.PushTextWrapPos();
        ImGui.TextUnformatted("自动寻路找A怪\n依赖插件: Vnavmesh、Lifestream");
        ImGui.PopTextWrapPos();
        ImGui.Separator();
        ImGui.TextUnformatted($"角色当前状态");
        ImGui.TextUnformatted($"数据中心: {DataManager.PlayerLocation.DcName}");
        ImGui.TextUnformatted($"服务器: {DataManager.PlayerLocation.WorldName}");
        ImGui.TextUnformatted($"地图: {DataManager.PlayerLocation.MapName}");
        ImGui.TextUnformatted($"地图ID: {DataManager.PlayerLocation.MapId}");
        ImGui.TextUnformatted($"分线: {DataManager.PlayerLocation.InstanceName}");
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

        var allSelected = DataManager.PlayerLocation.WorldsList.All(w => selectedWorld.ContainsKey(w) && selectedWorld[w]);

        ImGui.SameLine();
        if (ImGui.SmallButton(allSelected ? "取消全选" : "全选"))
        {
            foreach (var world in DataManager.PlayerLocation.WorldsList)
            {
                selectedWorld[world] = !allSelected;
            }
            SaveConfig();
        }

        using var table = ImRaii.Table("WorldTable", 3);
        if (table)
        {
            foreach (var world in DataManager.PlayerLocation.WorldsList)
            {
                ImGui.TableNextColumn();

                if (!selectedWorld.ContainsKey(world))
                    selectedWorld[world] = false;

                var isChecked = selectedWorld[world];
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
        RankAHuntHelper.Configuration.SelectedWorlds = selectedWorld;
        RankAHuntHelper.Configuration.SelectedExpansion = selectedExpansion;
        RankAHuntHelper.Configuration.SelectedMap = selectedMap;
        RankAHuntHelper.Configuration.Save();
    }

    private void LoadConfig()
    {
        enableCrossWorld = RankAHuntHelper.Configuration.EnableCrossWorld;

        selectedWorld = RankAHuntHelper.Configuration.SelectedWorlds
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        selectedExpansion = RankAHuntHelper.Configuration.SelectedExpansion
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        selectedMap = RankAHuntHelper.Configuration.SelectedMap
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new Dictionary<string, bool>(kvp.Value)
            );
    }
}
