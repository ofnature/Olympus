using System.Collections.Generic;
using System.Linq;

namespace Daedalus.Rotation.Common.Helpers;

/// <summary>
/// Formats the scheduler's per-candidate gate-fail reasons into a short, class-accurate "why nothing
/// fired" string for the Why Stuck panel. The scheduler already records exactly why each queued GCD was
/// rejected (Cooldown, ProcBuff, ComboStep, ActionStatus, Toggle, NotLearned, DispatchRejected, ...) —
/// this just surfaces it instead of the generic global-pause reason.
/// </summary>
public static class StuckReasonHelper
{
    private const int MaxShown = 4;

    /// <summary>
    /// Returns a "Stuck — ..." summary when the GCD was ready but no candidate dispatched and at least
    /// one candidate was rejected. Returns null when something fired, or when the queue was empty (the
    /// module's own debug state already explains why nothing was pushed: no target, out of combat, etc.).
    /// </summary>
    public static string? Describe(bool dispatched, IReadOnlyList<string> gateFailReasons)
    {
        if (dispatched || gateFailReasons.Count == 0)
            return null;

        var joined = string.Join("; ", gateFailReasons.Take(MaxShown));
        return gateFailReasons.Count > MaxShown
            ? $"Stuck — {joined}; +{gateFailReasons.Count - MaxShown} more"
            : $"Stuck — {joined}";
    }
}
