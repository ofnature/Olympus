using System;

namespace Olympus.Rotation.Common.Scheduling;

/// <summary>
/// Declarative charge-reserve policy for multi-charge abilities. When attached to an
/// <see cref="AbilityBehavior"/>, the scheduler keeps <see cref="HoldCharges"/> charges in
/// reserve while <see cref="InBurst"/> is false, so the ability never dumps its last charge
/// outside the burst window; inside the burst window every charge is spendable.
///
/// Generic by design: a job opts in entirely through <see cref="AbilityBehavior.ChargeHold"/>
/// (PLD Intervene, SAM Senei, NIN Dream Within a Dream, ...) with no scheduler changes.
/// Mirrors RSR's <c>CanUse(..., usedUp: inBurst)</c> charge-holding semantics.
/// </summary>
public sealed record ChargeHoldPolicy
{
    /// <summary>Number of charges to keep in reserve while <see cref="InBurst"/> is false.</summary>
    public required int HoldCharges { get; init; }

    /// <summary>
    /// Returns true when the rotation is in its burst window, where reserved charges may be
    /// spent. Receives the live rotation context so jobs can key off self-buffs (PLD Fight or
    /// Flight), target debuffs (NIN Kunai's Bane), gauge state, etc.
    /// </summary>
    public required Func<IRotationContext, bool> InBurst { get; init; }

    /// <summary>
    /// Common policy: reserve a single charge unless bursting.
    /// </summary>
    public static ChargeHoldPolicy HoldOneForBurst(Func<IRotationContext, bool> inBurst)
        => new() { HoldCharges = 1, InBurst = inBurst };
}
