using Olympus.Data;
using Olympus.Rotation.Common.Helpers;
using Olympus.Rotation.HermesCore.Context;
using Olympus.Services;
using Olympus.Services.Action;

namespace Olympus.Rotation.HermesCore.Helpers;

/// <summary>
/// Burst prep window: Shadow Walker up and Kunai's Bane / Trick Attack ready to fire.
/// </summary>
internal static class HermesBurstPrepHelper
{
    public static bool IsBurstPrepWindow(IHermesContext context)
    {
        if (!context.HasSuiton)
            return false;

        return IsKunaisBaneOrTrickReady(context.ActionService, context.Player.Level);
    }

    public static bool IsKunaisBaneOrTrickReady(IActionService actionService, byte level)
    {
        if (level >= NINActions.KunaisBane.MinLevel)
            return actionService.IsActionReady(NINActions.KunaisBane.ActionId);

        if (level >= NINActions.TrickAttack.MinLevel)
            return actionService.IsActionReady(NINActions.TrickAttack.ActionId);

        return false;
    }

    public static bool WouldHoldKunaisBane(IHermesContext context, IBurstWindowService? burstWindowService)
    {
        if (!HermesBurnHelper.ShouldPoolForRaidBurst(context))
            return false;

        if (BurstHoldHelper.ShouldHoldForPhaseTransition(context.TimelineService))
            return true;

        return BurstHoldHelper.ShouldHoldForBurst(
            burstWindowService,
            context.Configuration.Ninja.KunaisBaneHoldTime);
    }

    /// <summary>ABB: never hold combo GCDs — filler was causing multi-second dead air.</summary>
    public static bool ShouldHoldComboGcds(
        IHermesContext context,
        MudraHelper mudraHelper,
        IBurstWindowService? burstWindowService = null)
    {
        mudraHelper.ClearBurstPrepHold();
        return false;
    }

    /// <summary>Only force ST during active burst prep (Suiton + KB ready), not all of Shadow Walker.</summary>
    public static bool ShouldSuppressAoE(IHermesContext context, int enemyCount, int aoeThreshold)
        => IsBurstPrepWindow(context);
}
