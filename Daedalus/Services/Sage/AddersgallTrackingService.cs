using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Plugin.Services;

namespace Daedalus.Services.Sage;

/// <summary>
/// Tracks Sage's Addersgall resource.
/// Addersgall passively regenerates 1 stack every 20 seconds (up to 3 max).
/// Timer PAUSES when at max stacks - spending is required to continue generation.
/// Each Addersgall ability also restores 7% MP.
/// </summary>
public sealed class AddersgallTrackingService : IAddersgallTrackingService
{
    private readonly IJobGauges _jobGauges;

    public AddersgallTrackingService(IJobGauges jobGauges)
    {
        _jobGauges = jobGauges;
    }

    /// <summary>
    /// Maximum Addersgall stacks.
    /// </summary>
    public const int MaxStacks = 3;

    /// <summary>
    /// Seconds between passive stack generation.
    /// </summary>
    public const float RegenInterval = 20f;

    /// <summary>
    /// MP restored per Addersgall ability (as percentage).
    /// </summary>
    public const float MpRestorePercent = 0.07f;

    /// <summary>
    /// Gets the current number of Addersgall stacks.
    /// </summary>
    public int CurrentStacks => GetAddersgallStacks();

    /// <summary>
    /// Gets the time remaining until next Addersgall stack is generated.
    /// Returns 0 if at max stacks (timer is paused).
    /// </summary>
    public float TimerRemaining => GetAddersgallTimer();

    /// <summary>
    /// Returns true if we have any Addersgall stacks available.
    /// </summary>
    public bool HasStacks => CurrentStacks > 0;

    /// <summary>
    /// Returns true if Addersgall is at max stacks (timer paused).
    /// </summary>
    public bool IsAtMax => CurrentStacks >= MaxStacks;

    /// <summary>
    /// Returns true if the Addersgall timer is paused (at max stacks).
    /// </summary>
    public bool IsTimerPaused => IsAtMax;

    /// <summary>
    /// Returns true if we should spend Addersgall to prevent capping.
    /// </summary>
    /// <param name="windowSeconds">Seconds before stack to consider "about to cap".</param>
    public bool ShouldPreventCap(float windowSeconds = 3f)
    {
        var stacks = CurrentStacks;
        var timer = TimerRemaining;

        // If at max, we're already capped
        if (stacks >= MaxStacks)
            return true;

        // If timer is about to grant a stack and we'd hit max, should spend
        if (stacks == MaxStacks - 1 && timer <= windowSeconds && timer > 0)
            return true;

        return false;
    }

    /// <summary>
    /// Returns true if we have enough stacks for the specified cost.
    /// </summary>
    /// <param name="cost">Number of stacks required (typically 1).</param>
    public bool CanAfford(int cost = 1) => CurrentStacks >= cost;

    /// <summary>
    /// Returns true if we have more stacks than the reserve amount.
    /// </summary>
    /// <param name="reserveCount">Minimum stacks to keep reserved.</param>
    public bool HasStacksAboveReserve(int reserveCount) => CurrentStacks > reserveCount;

    /// <summary>
    /// Returns true if we should reserve stacks for emergency healing.
    /// </summary>
    /// <param name="reserveCount">Minimum stacks to reserve.</param>
    /// <param name="partyHealthCritical">Whether party health is in critical state.</param>
    public bool ShouldReserveForHealing(int reserveCount, bool partyHealthCritical)
    {
        // If party health is critical, don't reserve - use stacks for healing
        if (partyHealthCritical)
            return false;

        return CurrentStacks <= reserveCount;
    }

    /// <summary>
    /// Gets the Addersgall stack count from Dalamud's typed Sage gauge.
    /// </summary>
    private int GetAddersgallStacks()
    {
        try
        {
            return _jobGauges.Get<SGEGauge>().Addersgall;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets the Addersgall timer remaining in seconds.
    /// The game stores this in milliseconds; convert to seconds for callers.
    /// </summary>
    private float GetAddersgallTimer()
    {
        try
        {
            return _jobGauges.Get<SGEGauge>().AddersgallTimer / 1000f;
        }
        catch
        {
            return 0f;
        }
    }
}

/// <summary>
/// Interface for Addersgall tracking service.
/// </summary>
public interface IAddersgallTrackingService
{
    /// <summary>
    /// Gets the current number of Addersgall stacks.
    /// </summary>
    int CurrentStacks { get; }

    /// <summary>
    /// Gets the time remaining until next Addersgall stack is generated.
    /// </summary>
    float TimerRemaining { get; }

    /// <summary>
    /// Returns true if we have any Addersgall stacks available.
    /// </summary>
    bool HasStacks { get; }

    /// <summary>
    /// Returns true if Addersgall is at max stacks.
    /// </summary>
    bool IsAtMax { get; }

    /// <summary>
    /// Returns true if the Addersgall timer is paused (at max stacks).
    /// </summary>
    bool IsTimerPaused { get; }

    /// <summary>
    /// Returns true if we should spend Addersgall to prevent capping.
    /// </summary>
    bool ShouldPreventCap(float windowSeconds = 3f);

    /// <summary>
    /// Returns true if we have enough stacks for the specified cost.
    /// </summary>
    bool CanAfford(int cost = 1);

    /// <summary>
    /// Returns true if we have more stacks than the reserve amount.
    /// </summary>
    bool HasStacksAboveReserve(int reserveCount);

    /// <summary>
    /// Returns true if we should reserve stacks for emergency healing.
    /// </summary>
    bool ShouldReserveForHealing(int reserveCount, bool partyHealthCritical);
}
