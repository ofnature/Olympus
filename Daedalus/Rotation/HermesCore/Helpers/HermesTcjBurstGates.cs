using Daedalus.Data;
using Daedalus.Rotation.HermesCore.Context;
using Daedalus.Services.Action;

namespace Daedalus.Rotation.HermesCore.Helpers;

/// <summary>
/// RSR AttackAbility Ten Chi Jin oGCD gates (Reborn + Beiruta parity).
/// </summary>
internal static class HermesTcjBurstGates
{
    public const float TenRecastGuardSeconds = 30f;
    public const float BurstPhaseRecastRemainSeconds = 45f;

    public static bool CanPushTenChiJinOgcd(IHermesContext context)
    {
        if (context.HasKassatsu) return false;
        if (context.HasTenChiJin) return false;
        if (context.HasGameMudraStatus) return false;
        if (!context.InTrickAttack && !IsInBurstPhase(context)) return false;
        if (context.HasSuiton) return false;
        if (!PassesTenChargeGuard(context.ActionService)) return false;
        return context.ActionService.IsActionReady(NINActions.TenChiJin.ActionId);
    }

    public static bool IsInBurstPhase(IHermesContext context)
    {
        var burstAction = context.Player.Level >= NINActions.KunaisBane.MinLevel
            ? NINActions.KunaisBane
            : NINActions.TrickAttack;
        return context.InCombat
            && context.ActionService.GetCooldownRemaining(burstAction.ActionId) > BurstPhaseRecastRemainSeconds;
    }

    /// <summary>RSR !TenPvE.Cooldown.ElapsedAfter(30).</summary>
    public static bool PassesTenChargeGuard(IActionService actionService)
    {
        if (actionService.IsActionReady(NINActions.Ten.ActionId)) return true;
        return actionService.GetRecastTimeElapsed(NINActions.Ten.ActionId) < TenRecastGuardSeconds;
    }
}
