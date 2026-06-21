using Olympus.Services.Positional;

namespace Olympus.Rotation.HermesCore.Helpers;

/// <summary>
/// RSR NIN_Reborn GeneralGCD Kazematoi finisher routing.
/// Shared by DamageModule, Hermes.GetNextRequiredPositional, and NinjaPositionalAnticipationProvider.
/// </summary>
internal static class HermesKazematoiRules
{
    /// <summary>Armor Crush fallback while Kazematoi &lt; this value (RSR: Kazematoi &lt; 4).</summary>
    public const int ArmorCrushFallbackThreshold = 4;

    public static bool ShouldBuildWithArmorCrush(int kazematoi) => kazematoi == 0;

    public static bool ShouldSpendWithAeolian(int kazematoi) => kazematoi > 0;

    public static bool ShouldFallbackArmorCrush(int kazematoi) => kazematoi < ArmorCrushFallbackThreshold;

    /// <summary>Positional needed at combo step 3 (after Gust Slash).</summary>
    public static PositionalType? GetFinisherPositional(int kazematoi)
    {
        if (ShouldBuildWithArmorCrush(kazematoi))
            return PositionalType.Flank;
        if (ShouldSpendWithAeolian(kazematoi))
            return PositionalType.Rear;
        return null;
    }
}
