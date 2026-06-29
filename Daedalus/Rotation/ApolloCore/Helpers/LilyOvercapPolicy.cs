namespace Daedalus.Rotation.ApolloCore.Helpers;

/// <summary>
/// Pure decision for the WHM Lily cap-prevention dump (RSR LilyAfterGCD parity). Extracted so it can be
/// unit-tested without the Dalamud gauge.
/// </summary>
public static class LilyOvercapPolicy
{
    public const int MaxLilies = 3;

    /// <summary>
    /// "Nearly full" window: at 2/3 Lilies, dump when the next Lily ticks within roughly one GCD so we
    /// never sit at 3/3 wasting a regen. ~2.5s default.
    /// </summary>
    public const float NearlyFullOvercapSeconds = 2.5f;

    /// <summary>
    /// True when a Lily should be spent to avoid wasting regen: at 3/3 (already capped), or at 2/3 with
    /// the next Lily about to tick (within <see cref="NearlyFullOvercapSeconds"/>).
    /// </summary>
    public static bool ShouldDump(int lilyCount, float secondsUntilNextLily) =>
        lilyCount >= MaxLilies
        || (lilyCount == MaxLilies - 1 && secondsUntilNextLily <= NearlyFullOvercapSeconds);
}
