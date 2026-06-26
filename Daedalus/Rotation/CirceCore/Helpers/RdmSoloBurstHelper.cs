using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using System.Numerics;
using Daedalus.Data;
using Daedalus.Rotation.CirceCore.Context;
using Daedalus.Services;

namespace Daedalus.Rotation.CirceCore.Helpers;

/// <summary>
/// Solo/trust burst sequencing for Red Mage: Manafication → Embolden → melee combo.
/// Active when <see cref="IBurstWindowService.UseSoloBurstFallback"/> is true
/// (computed on read — no frame lag vs <see cref="IBurstWindowService.Update"/>).
/// </summary>
public static class RdmSoloBurstHelper
{
    public static bool IsSoloBurstMode(IBurstWindowService? burstWindowService) =>
        burstWindowService?.UseSoloBurstFallback == true;

    /// <summary>
    /// Target HP and pack size gate — avoids wasting raid buffs on dying packs.
    /// </summary>
    public static bool IsBurstPackViable(ICirceContext context, IBattleChara? target, IPlayerCharacter player)
    {
        if (target is not IBattleNpc battleNpc)
            return false;

        var cfg = context.Configuration.RedMage;
        if (battleNpc.MaxHp > 0)
        {
            var hpPercent = (float)battleNpc.CurrentHp / battleNpc.MaxHp;
            if (hpPercent < cfg.SoloBurstMinTargetHpPercent)
                return false;
        }

        var enemyCount = context.TargetingService.CountEnemiesInRangeOfTarget(5f, battleNpc, player);
        return enemyCount >= cfg.SoloBurstMinEnemies;
    }

    /// <summary>
    /// Both major burst oGCDs ready, or one ready with the other coming off CD within the window.
    /// </summary>
    public static bool AreBurstCooldownsPaired(ICirceContext context, float windowSeconds)
    {
        if (context.EmboldenReady && context.ManaficationReady)
            return true;

        var actions = context.ActionService;
        if (context.EmboldenReady && !context.ManaficationReady)
        {
            var cd = actions.GetCooldownRemaining(RDMActions.Manafication.ActionId);
            return cd >= 0f && cd <= windowSeconds;
        }

        if (context.ManaficationReady && !context.EmboldenReady)
        {
            var cd = actions.GetCooldownRemaining(RDMActions.Embolden.ActionId);
            return cd >= 0f && cd <= windowSeconds;
        }

        return false;
    }

    /// <summary>
    /// Mana threshold to begin the solo burst pair (Manafication first).
    /// </summary>
    public static bool IsSoloBurstManaReadyForPairStart(ICirceContext context)
    {
        var cfg = context.Configuration.RedMage;
        if (context.LowerMana >= cfg.SoloBurstIdealMinMana)
            return true;

        return context.LowerMana >= cfg.MeleeComboMinMana
               && context.EmboldenReady
               && context.ManaficationReady;
    }

    /// <summary>
    /// Hold Riposte/Moulinet until Manafication and Embolden are both active during solo burst setup.
    /// </summary>
    public static bool ShouldHoldMeleeForSoloBurstChain(ICirceContext context, IBurstWindowService? burstWindowService)
    {
        if (!IsSoloBurstMode(burstWindowService))
            return false;

        if (context.HasManafication && context.HasEmbolden)
            return false;

        var cfg = context.Configuration.RedMage;
        var burstActive = context.HasManafication || context.HasEmbolden;
        var burstImminent = AreBurstCooldownsPaired(context, cfg.SoloBurstPairCooldownSeconds)
                            && (context.ManaficationReady || context.EmboldenReady);

        return burstActive || burstImminent;
    }

    /// <summary>
    /// True when player is outside the 3y melee entry range after accounting for both hitbox radii.
    /// </summary>
    public static bool IsOutsideMeleeEntryRange(IPlayerCharacter player, IBattleChara? target)
    {
        if (target == null)
            return false;

        var centerDistance = Vector3.Distance(player.Position, target.Position);
        var edgeDistance = centerDistance - player.HitboxRadius - target.HitboxRadius;
        return edgeDistance > 3f;
    }

    /// <summary>
    /// Whether solo burst should use Corps-a-corps first to enter melee range before Riposte.
    /// </summary>
    public static bool ShouldGapCloseForMeleeEntry(
        ICirceContext context,
        IBurstWindowService? burstWindowService,
        IBattleChara? target)
    {
        if (!IsSoloBurstMode(burstWindowService))
            return false;

        if (ShouldHoldMeleeForSoloBurstChain(context, burstWindowService))
            return false;

        if (!context.CanStartMeleeCombo || context.IsInMeleeCombo || context.IsInMoulinetCombo)
            return false;

        if (context.CorpsACorpsCharges <= 0)
            return false;

        var hpPercent = context.Player.MaxHp > 0
            ? (float)context.Player.CurrentHp / context.Player.MaxHp
            : 1f;
        if (hpPercent < context.Configuration.RedMage.MeleeDashMinHpPercent)
            return false;

        return IsOutsideMeleeEntryRange(context.Player, target);
    }
}
