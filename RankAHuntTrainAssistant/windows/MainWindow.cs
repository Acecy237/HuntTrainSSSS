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
using RankAHuntTrainAssistant.Data;
using RankAHuntTrainAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static FFXIVClientStructs.FFXIV.Client.Network.NetworkModuleProxy.Delegates;
using static RankAHuntTrainAssistant.Data.MapMetaData;

namespace RankAHuntTrainAssistant.windows;

public class MainWindow : Window, IDisposable
{
    private bool enableCrossWorld = false;
    private Dictionary<string, bool> worldToggles = new();
    private Dictionary<string, bool> versionsToggles = new();
    private Dictionary<string, Dictionary<string, bool>> mapToggles = new();

    public MainWindow(Plugin plugin)
        : base("RankAHuntTrainAssistant##aht", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        LoadFromConfig();
    }

    public void Dispose() {
    
    }
    private void LoadFromConfig()
    {
        var config = Plugin.Configuration;

        enableCrossWorld = config.EnableCrossWorld;
        worldToggles = new(config.WorldToggles);
        versionsToggles = new(config.VersionSelections);

        mapToggles = new();
        foreach (var kv in config.MapSelections)
        {
            mapToggles[kv.Key] = new(kv.Value);
        }
    }
    private void SaveConfig()
    {
        Plugin.Configuration.EnableCrossWorld = enableCrossWorld;
        Plugin.Configuration.WorldToggles = new(worldToggles);
        Plugin.Configuration.VersionSelections = new(versionsToggles);
        Plugin.Configuration.MapSelections = new();
        foreach (var kv in mapToggles)
        {
            Plugin.Configuration.MapSelections[kv.Key] = new(kv.Value);
        }
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
        ImGui.TextUnformatted($"数据中心: {DCWorlds.DcName}");
        ImGui.TextUnformatted($"服务器: {DCWorlds.WorldName}");
        ImGui.TextUnformatted($"地图ID: {MapInfo.TerritoryId}");
        ImGui.TextUnformatted($"地图: {MapInfo.MapName}");
        ImGui.TextUnformatted($"分线: {MapInfo.InstanceName}");
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
            foreach (var world in DCWorlds.WorldsList)
            {
                ImGui.TableNextColumn();

                if (!worldToggles.ContainsKey(world))
                    worldToggles[world] = false;

                bool value = worldToggles[world];
                if (ImGui.Checkbox(world, ref value))
                {
                    worldToggles[world] = value;
                    SaveConfig();
                }
            }
            ImGui.EndTable();
        }
        ImGui.Separator();
    }

    private void DrawVersionSelector()
    {
        ImGui.TextUnformatted("选择版本:");
        ImGui.Spacing();

        for (int i = 0; i < MapMetaData.VersionOptions.Length; i++)
        {
            var version = MapMetaData.VersionOptions[i];
            if (!versionsToggles.TryGetValue(version, out var isChecked))
            {
                isChecked = false;
                versionsToggles[version] = false;
            }

            if (ImGui.Checkbox(version, ref isChecked))
            {
                versionsToggles[version] = isChecked;
                SaveConfig();
            }

            if (isChecked)
            {
                var mapNames = MapMetaData.GetMapNames(version).ToArray();

                if (!mapToggles.ContainsKey(version))
                {
                    mapToggles[version] = new();
                    foreach (var map in mapNames)
                    {
                        mapToggles[version][map] = false;
                    }
                }

                ImGui.Indent();

                if (ImGui.BeginTable($"MapTable_{version}", 3))
                {
                    foreach (var map in mapNames)
                    {
                        ImGui.TableNextColumn();

                        if (!mapToggles[version].ContainsKey(map))
                            mapToggles[version][map] = false;

                        bool mapChecked = mapToggles[version][map];
                        if (ImGui.Checkbox(map, ref mapChecked))
                        {
                            mapToggles[version][map] = mapChecked;
                            SaveConfig();
                        }
                    }
                    ImGui.EndTable();
                }
                ImGui.Unindent();
            }
        }
    }


}
