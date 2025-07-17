using System;

namespace RankAHuntTrainAssistant.StaticData;

public static class ExpansionData
{
    public enum Expansion { 暗影之逆焰, 晓月之终途, 金曦之遗辉 }
    public static string ToDisplayName(this Expansion exp) => exp switch
    {
        Expansion.暗影之逆焰 => "暗影之逆焰",
        Expansion.晓月之终途 => "晓月之终途",
        Expansion.金曦之遗辉 => "金曦之遗辉",
        _ => exp.ToString()
    };
    public static Expansion[] Expansions => (Expansion[])Enum.GetValues(typeof(Expansion));
}
