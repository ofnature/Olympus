using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;
using Daedalus.Rotation.Common.Helpers;

namespace Daedalus.Services.Targeting;

/// <summary>
/// Decides whether a hostile is eligible for auto-targeting when the personal InCombat flag
/// is missing — common in alliance raids where another party tagged the mob first.
/// </summary>
internal static class EnemyEngagementPolicy
{
    internal static bool IsPlayerEffectivelyInCombat(
        IPlayerCharacter player,
        Configuration configuration,
        IPartyList partyList,
        IObjectTable objectTable)
    {
        if ((player.StatusFlags & StatusFlags.InCombat) != 0)
            return true;

        if (configuration.EnableOnPartyInCombat
            && PartyCombatHelper.IsAnyGroupMemberInCombat(player, partyList, objectTable))
            return true;

        return false;
    }

    internal static bool ShouldRelaxEnemyInCombatRequirement(
        Configuration configuration,
        IPlayerCharacter player,
        IPartyList partyList,
        IObjectTable objectTable)
    {
        if (configuration.Targeting.IncludeHostilesWithoutPersonalCombatFlag)
            return true;

        if (configuration.EnableOnPartyInCombat
            && PartyCombatHelper.IsAnyGroupMemberInCombat(player, partyList, objectTable))
            return true;

        return false;
    }

    /// <summary>
    /// True when an enemy should be considered for aggregate strategies, AoE counts, and combat retarget.
    /// </summary>
    internal static bool ShouldIncludeEnemyForTargeting(
        IBattleNpc enemy,
        ulong currentTargetId,
        bool playerEffectivelyInCombat,
        bool relaxEnemyInCombatRequirement)
    {
        if ((enemy.StatusFlags & StatusFlags.InCombat) != 0)
            return true;

        if (currentTargetId != 0 && enemy.GameObjectId == currentTargetId)
            return true;

        if (!playerEffectivelyInCombat)
            return false;

        return relaxEnemyInCombatRequirement;
    }
}
