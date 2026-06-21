using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using Olympus.Compat;
using Olympus.Data;

namespace Olympus.Services.Targeting;

/// <summary>
/// Filters battle NPCs that look hostile but cannot be damaged (escort/protect objectives, etc.).
/// Uses the game's action-target validation rather than StatusFlags alone.
/// </summary>
public static class EnemyAttackability
{
    /// <summary>
    /// Probe actions covering melee and ranged single-target damage validation.
    /// </summary>
    private static readonly uint[] DamageProbeActionIds =
    [
        ActionIds.HeavySwing,
        BLMActions.Blizzard.ActionId,
    ];

    public static bool IsExcludedBattleNpcKind(IBattleNpc npc)
    {
        var kind = (byte)npc.BattleNpcKind;
        return kind is BattleNpcKinds.Pet
            or BattleNpcKinds.Chocobo
            or BattleNpcKinds.NpcPartyMember;
    }

    /// <summary>
    /// True when the player can use a damage action on this battle NPC.
    /// </summary>
    public static bool IsPlayerAttackable(IGameObject target)
    {
        if (target is not IBattleNpc npc)
            return false;

        if (IsExcludedBattleNpcKind(npc))
            return false;

        if (!target.IsTargetable || target.IsDead)
            return false;

        return CanUseDamageActionOnTarget(target);
    }

    private static unsafe bool CanUseDamageActionOnTarget(IGameObject target)
    {
        try
        {
            var targetStruct = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)target.Address;
            if (targetStruct == null)
                return false;

            foreach (var actionId in DamageProbeActionIds)
            {
                if (ActionManager.CanUseActionOnTarget(actionId, targetStruct))
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
