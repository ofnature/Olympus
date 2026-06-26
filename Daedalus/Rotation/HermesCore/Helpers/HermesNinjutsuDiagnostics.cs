using Daedalus.Data;
using Daedalus.Rotation.Common.Helpers;
using Daedalus.Rotation.HermesCore.Context;

namespace Daedalus.Rotation.HermesCore.Helpers;

/// <summary>
/// Live gate evaluation for Hermes debug tab — mirrors NinjutsuModule / TCJ oGCD logic without changing it.
/// </summary>
internal static class HermesNinjutsuDiagnostics
{
    /// <summary>
    /// During AoE pulls, prefer Doton/combo over Suiton prep — KB can fire after the pack thins.
    /// Also defers Suiton at 2+ targets until the first Doton is placed (trash opener).
    /// </summary>
    public static bool ShouldDeferSuitonForAoE(IHermesContext context, byte level, int enemyCount)
    {
        var ninja = context.Configuration.Ninja;
        if (!ninja.EnableAoERotation)
            return false;

        if (enemyCount >= ninja.AoEMinTargets)
            return true;

        if (ninja.UseDotonForAoE
            && level >= NINActions.Doton.MinLevel
            && enemyCount >= 2
            && !HasDotonGroundDoT(context))
            return true;

        return false;
    }

    /// <summary>
    /// While Doton is ticking in an AoE pull, skip Katon/Raiton filler — combo GCDs are higher value (ABB).
    /// </summary>
    public static bool ShouldSkipAoEFillerForActiveDoton(
        IHermesContext context, byte level, int enemyCount)
    {
        if (context.HasKassatsu)
            return false;

        if (!ShouldDeferSuitonForAoE(context, level, enemyCount))
            return false;

        return level >= NINActions.Doton.MinLevel && HasDotonGroundDoT(context);
    }

    public static bool HasDotonGroundDoT(IHermesContext context)
        => BaseStatusHelper.HasStatus(context.Player, NINActions.StatusIds.Doton)
           || (context.MudraHelper?.HasDotonActiveLatch ?? false);

    public static bool EvaluateNeedsSuiton(
        IHermesContext context, byte level, int enemyCount, out string reason)
    {
        if (level < NINActions.Suiton.MinLevel)
        {
            reason = $"Level {level} < Suiton ({NINActions.Suiton.MinLevel})";
            return false;
        }

        if (context.HasSuiton)
        {
            reason = "Shadow Walker active";
            return false;
        }

        if (ShouldDeferSuitonForAoE(context, level, enemyCount))
        {
            reason = HasDotonGroundDoT(context)
                ? $"AoE pull ({enemyCount} enemies) — Doton/combo priority"
                : $"AoE pull ({enemyCount} enemies) — opening Doton before Suiton";
            return false;
        }

        var kunaiAction = level >= NINActions.KunaisBane.MinLevel
            ? NINActions.KunaisBane
            : NINActions.TrickAttack;

        if (!context.ActionService.IsActionReady(kunaiAction.ActionId))
        {
            reason = $"{kunaiAction.Name} not ready (on CD)";
            return false;
        }

        reason = $"{kunaiAction.Name} ready, no Shadow Walker";
        return true;
    }

    public static bool EvaluateShouldStartNinjutsu(
        IHermesContext context,
        byte level,
        int enemyCount,
        out string blockReason)
    {
        blockReason = "";

        if (!context.Configuration.Ninja.EnableNinjutsu)
        {
            blockReason = "EnableNinjutsu off";
            return false;
        }

        if (level < NINActions.Ten.MinLevel)
        {
            blockReason = $"Level {level} < Ten ({NINActions.Ten.MinLevel})";
            return false;
        }

        if (EvaluateNeedsSuiton(context, level, enemyCount, out var suitonReason))
        {
            blockReason = context.ActionService.IsActionReady(NINActions.Ten.ActionId)
                ? $"NeedsSuiton ({suitonReason}): would start Suiton"
                : $"NeedsSuiton ({suitonReason}): queued (waiting for Ten)";
            return true;
        }

        if (HermesBurstPrepHelper.IsBurstPrepWindow(context))
        {
            blockReason = "Burst prep — Kunai's Bane pending";
            return false;
        }

        if (context.HasKassatsu)
        {
            blockReason = HermesNinjutsuMudraExecutor.IsTenPressable(context)
                ? "Kassatsu path (would start)"
                : "Kassatsu path (queued, waiting for Ten)";
            return true;
        }

        if (ShouldSkipAoEFillerForActiveDoton(context, level, enemyCount))
        {
            blockReason = "AoE — Doton active, combo priority";
            return false;
        }

        if (!HermesNinjutsuMudraExecutor.IsTenPressable(context))
        {
            blockReason = "Ten not ready";
            return false;
        }

        if (!context.CanExecuteGcd)
        {
            blockReason = "GCD not ready (CanExecuteGcd false)";
            return false;
        }

        blockReason = "Filler path (would start)";
        return true;
    }

    public static bool TryGetTcjOgcdBlockReason(IHermesContext context, out string blockReason)
    {
        if (context.HasKassatsu)
        {
            blockReason = "Has Kassatsu";
            return false;
        }

        if (context.HasTenChiJin)
        {
            blockReason = "TCJ buff active";
            return false;
        }

        if (context.HasGameMudraStatus)
        {
            blockReason = "Game mudra status (496)";
            return false;
        }

        var inBurstPhase = HermesTcjBurstGates.IsInBurstPhase(context);
        if (!context.InTrickAttack && !inBurstPhase)
        {
            blockReason = "Not InTrickAttack and not InBurstPhase";
            return false;
        }

        if (context.HasSuiton)
        {
            blockReason = "Shadow Walker active";
            return false;
        }

        if (!HermesTcjBurstGates.PassesTenChargeGuard(context.ActionService))
        {
            blockReason = "Ten charge guard (recast > 30s)";
            return false;
        }

        if (!context.ActionService.IsActionReady(NINActions.TenChiJin.ActionId))
        {
            blockReason = "TCJ oGCD not ready";
            return false;
        }

        blockReason = "";
        return true;
    }
}
