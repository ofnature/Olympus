using System.Numerics;
using Daedalus.Services.Action;
using Daedalus.Services.Positional;

namespace Daedalus.Services.Positional.Navigation;

/// <summary>
/// vNav melee approach during NIN burst prep (Suiton / Shadow Walker window, out of Kunai's Bane range).
/// Complements <see cref="PositionalMovementService"/> positional arcs — runs only when no positional path is active.
/// </summary>
public sealed class NinjaBurstApproachService
{
    private readonly IVNavService _vNav;
    private readonly IBossModSafetyService _bossModSafety;

    public NinjaBurstApproachService(IVNavService vNav, IBossModSafetyService bossModSafety)
    {
        _vNav = vNav;
        _bossModSafety = bossModSafety;
    }

    public PositionalMovementState State { get; private set; }

    public void Update(in NinjaBurstApproachRequest request)
    {
        _bossModSafety.BeginUpdateSnapshot();

        if (_vNav.IsPathRunning && _bossModSafety.ShouldAbortMovement())
        {
            Cancel("new mechanic telegraph");
            return;
        }

        if (!request.Enabled)
        {
            SetSkipped("disabled", stopPath: true);
            return;
        }

        if (!request.InCombat)
        {
            SetSkipped("not in combat", stopPath: true);
            return;
        }

        if (!request.BurstPrepActive)
        {
            SetSkipped("not in burst prep", stopPath: true);
            return;
        }

        if (request.Target is not { } target)
        {
            if (_vNav.IsPathRunning && !request.AlreadyInMeleeRange)
            {
                State = new PositionalMovementState(
                    PositionalMovementPhase.Moving,
                    null,
                    State.Destination,
                    SkipReason: "burst approach (awaiting target)");
                return;
            }

            SetSkipped("no approach target", stopPath: true);
            return;
        }

        if (request.PositionalPathActive && request.AlreadyInMeleeRange)
        {
            SetSkipped("positional path active", stopPath: true);
            return;
        }

        if (request.AlreadyInMeleeRange)
        {
            SetSkipped("already in melee range", stopPath: true);
            return;
        }

        if (target.HasPositionalImmunity)
        {
            SetSkipped("target positional immunity", stopPath: true);
            return;
        }

        // Hold an in-progress approach path until vNav finishes. Without this we'd re-issue
        // PathfindAndMoveCloseTo every frame while the path is computing or running — the
        // vNav "stutter step". Re-arm only once the path has fully completed.
        if (_vNav.IsPathRunning || _vNav.IsPathfindInProgress)
        {
            State = new PositionalMovementState(
                PositionalMovementPhase.Moving, null, State.Destination, "burst melee approach");
            return;
        }

        // Dead-band: once the player is within melee stand distance ± arrival tolerance, suppress
        // re-pathing. Without this, micro target position shifts cause a new path every frame.
        var standDistance = PositionalStandCalculator.MaxMeleeStandDistance(
            target.HitboxRadius, request.PlayerHitboxRadius);
        var currentDistance = HorizontalDistanceTo(request.PlayerPosition, target.Position);
        if (currentDistance <= standDistance + PositionalMovementConstants.BurstApproachArrivalToleranceYalms)
        {
            SetSkipped("within melee dead-band", stopPath: true);
            return;
        }

        // NIN abilities are instant — vNav can start during animation lock / while weaving.
        var standRequest = new MeleeApproachStandRequest(
            PlayerPosition: request.PlayerPosition,
            PlayerHitboxRadius: request.PlayerHitboxRadius,
            TargetPosition: target.Position,
            TargetHitboxRadius: target.HitboxRadius);

        var destination = PositionalStandCalculator.CalculateBurstMeleeApproach(in standRequest);
        destination = _vNav.SnapToFloor(destination);

        var safety = _bossModSafety.QueryPositionSafety(destination);
        if (safety is PositionSafety.Unsafe or PositionSafety.Imminent)
        {
            SetSkipped($"unsafe destination ({safety})", stopPath: true);
            return;
        }

        if (!_bossModSafety.IsSegmentSafe(request.PlayerPosition, destination))
        {
            SetSkipped("dash segment unsafe", stopPath: true);
            return;
        }

        if (TryQueueBurstMove(destination, out var skipReason))
            return;

        SetSkipped(skipReason ?? "vNav unavailable", stopPath: false);
    }

    private bool TryQueueBurstMove(Vector3 destination, out string? skipReason)
    {
        skipReason = null;

        var moveResult = QueueBurstMove(destination);
        if (moveResult == VNavMoveResult.Queued)
        {
            State = new PositionalMovementState(
                PositionalMovementPhase.Moving,
                null,
                destination,
                SkipReason: "burst melee approach");
            return true;
        }

        skipReason = $"vNav unavailable ({moveResult})";
        return false;
    }

    private VNavMoveResult QueueBurstMove(Vector3 destination)
        => _vNav.PathfindAndMoveCloseTo(
            destination,
            fly: false,
            toleranceYalms: PositionalMovementConstants.BurstApproachArrivalToleranceYalms);

    public void Cancel(string reason)
    {
        _vNav.Stop();
        State = new PositionalMovementState(
            PositionalMovementPhase.Aborted,
            State.TargetZone,
            State.Destination,
            reason);
    }

    private void SetSkipped(string? reason, bool stopPath = true)
    {
        if (stopPath)
            StopOwnedPathIfActive();

        State = new PositionalMovementState(
            PositionalMovementPhase.Skipped,
            SkipReason: reason);
    }

    private static float HorizontalDistanceTo(Vector3 from, Vector3 to)
    {
        var dx = from.X - to.X;
        var dz = from.Z - to.Z;
        return System.MathF.Sqrt((dx * dx) + (dz * dz));
    }

    private void StopOwnedPathIfActive()
    {
        if (State.Phase == PositionalMovementPhase.Moving && _vNav.IsPathRunning)
            _vNav.Stop();
    }
}

public readonly record struct NinjaBurstApproachRequest(
    bool Enabled,
    bool InCombat,
    /// <summary>Shadow Walker / Suiton burst window (includes post-Suiton status latch).</summary>
    bool BurstPrepActive,
    bool AlreadyInMeleeRange,
    bool PositionalPathActive,
    Vector3 PlayerPosition,
    float PlayerHitboxRadius,
    PositionalMovementTarget? Target,
    IActionService ActionService);
