using System;
using Olympus.Data;
using Olympus.Services.Action;

namespace Olympus.Rotation.HermesCore.Helpers;

/// <summary>
/// Live Ten mudra charge state from ActionManager (RSR usedUp parity).
/// </summary>
internal readonly struct TenMudraChargeSnapshot
{
    public uint AdjustedActionId { get; init; }
    public uint CurrentCharges { get; init; }
    public ushort MaxCharges { get; init; }
    /// <summary>Time until the next charge finishes recharging (0 when fully stocked).</summary>
    public float NextChargeRemaining { get; init; }
    /// <summary>Full per-charge recast duration from the game (when a recharge is active).</summary>
    public float ChargeRecastTotal { get; init; }
    /// <summary>RSR CanUse(usedUp: true) — charge available or CD will clear before GCD ends.</summary>
    public bool IsPressable { get; init; }
    /// <summary>Estimated seconds until <see cref="IsPressable"/> becomes true (0 if already pressable).</summary>
    public float SecondsUntilPressable { get; init; }
}

internal static class TenMudraChargeTracker
{
    /// <summary>Per-charge recharge at max level (Patch 7.5). ActionManager is authoritative; this labels debug UI.</summary>
    public const float KnownChargeRecastSeconds = 20f;

    public static TenMudraChargeSnapshot GetSnapshot(IActionService actionService, byte playerLevel)
    {
        var tenAdj = actionService.GetAdjustedActionId(NINActions.Ten.ActionId);
        var charges = actionService.GetCurrentCharges(tenAdj);
        var maxCharges = actionService.GetMaxCharges(tenAdj, playerLevel);
        var cdRem = actionService.GetCooldownRemaining(tenAdj);
        var gcdRem = actionService.GcdRemaining;
        var elapsed = actionService.GetRecastTimeElapsed(tenAdj);

        var isPressable = charges > 0 || cdRem <= gcdRem;
        var secondsUntilPressable = charges > 0
            ? 0f
            : Math.Max(0f, cdRem - gcdRem);

        var recastTotal = cdRem > 0f ? elapsed + cdRem : 0f;
        if (recastTotal <= 0f && cdRem > 0f)
            recastTotal = KnownChargeRecastSeconds;

        return new TenMudraChargeSnapshot
        {
            AdjustedActionId = tenAdj,
            CurrentCharges = charges,
            MaxCharges = maxCharges,
            NextChargeRemaining = cdRem,
            ChargeRecastTotal = recastTotal,
            IsPressable = isPressable,
            SecondsUntilPressable = secondsUntilPressable,
        };
    }

    public static string FormatWaitSummary(in TenMudraChargeSnapshot snapshot)
    {
        if (snapshot.IsPressable)
            return $"Ten ready ({snapshot.CurrentCharges}/{snapshot.MaxCharges})";

        if (snapshot.SecondsUntilPressable > 0.05f)
        {
            return snapshot.NextChargeRemaining > snapshot.SecondsUntilPressable + 0.05f
                ? $"Ten in {snapshot.SecondsUntilPressable:F1}s ({snapshot.CurrentCharges}/{snapshot.MaxCharges}, next +{snapshot.NextChargeRemaining:F1}s)"
                : $"Ten in {snapshot.SecondsUntilPressable:F1}s ({snapshot.CurrentCharges}/{snapshot.MaxCharges})";
        }

        return $"Ten on CD {snapshot.NextChargeRemaining:F1}s ({snapshot.CurrentCharges}/{snapshot.MaxCharges})";
    }
}
