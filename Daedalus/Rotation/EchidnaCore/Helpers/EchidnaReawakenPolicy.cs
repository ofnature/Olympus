namespace Daedalus.Rotation.EchidnaCore.Helpers;

/// <summary>
/// Pure entry gate for Reawaken's self-buff readiness.
///
/// Reawaken is a ~13s burst phase, so the haste (<c>Swiftscaled</c>) and damage (<c>Hunter's Instinct</c>)
/// self-buffs must have enough duration left to cover it. It is deliberately NOT gated on Noxious Gnash:
/// that is a per-target debuff, so after a target swap in a pack it reads 0 and would block the whole burst,
/// overcapping Serpent's Offering. RSR (VPR_Reborn) likewise checks only the Swift/Hunter timers for Reawaken
/// entry and maintains Noxious Gnash separately (via the Vicewinder path). Mirrors the RPR Enshroud/Death's
/// Design decoupling.
/// </summary>
public static class EchidnaReawakenPolicy
{
    /// <summary>Buffs must outlast the Reawaken window; 10s comfortably covers ~13s shortened by Swiftscaled.</summary>
    public const float MinBuffSeconds = 10f;

    public static bool BuffsReadyForReawaken(
        bool hasHuntersInstinct,
        float huntersInstinctRemaining,
        bool hasSwiftscaled,
        float swiftscaledRemaining,
        float minBuffSeconds = MinBuffSeconds)
    {
        if (!hasHuntersInstinct || huntersInstinctRemaining < minBuffSeconds)
            return false;
        if (!hasSwiftscaled || swiftscaledRemaining < minBuffSeconds)
            return false;
        return true;
    }
}
