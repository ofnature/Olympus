using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;

namespace Daedalus.Rotation.Common.Helpers;

/// <summary>
/// Detects whether allies are in combat so the rotation can assist before the local player has aggro.
/// </summary>
internal static class PartyCombatHelper
{
    /// <summary>
    /// True when any party member (or Trust ally) other than the player has <see cref="StatusFlags.InCombat"/>.
    /// </summary>
    public static bool IsAnyGroupMemberInCombat(
        IPlayerCharacter player,
        IPartyList partyList,
        IObjectTable objectTable)
    {
        if (partyList.Length > 0)
        {
            foreach (var member in partyList)
            {
                if (member.EntityId == player.EntityId)
                    continue;

                if (objectTable.SearchByEntityId(member.EntityId) is not IBattleChara chara)
                    continue;

                if (chara.IsDead)
                    continue;

                if ((chara.StatusFlags & StatusFlags.InCombat) != 0)
                    return true;
            }

            return false;
        }

        // Trust / squadron / duty companion allies — PartyList is empty in Trust/squadron content.
        // Check any non-hostile BattleNpc for InCombat (trusts, squadrons, duty support NPCs).
        foreach (var obj in objectTable)
        {
            if (obj is not IBattleNpc npc)
                continue;
            if (npc.CurrentHp == 0 || npc.MaxHp == 0)
                continue;
            if ((byte)npc.BattleNpcKind == Daedalus.Compat.BattleNpcKinds.Combatant)
                continue;
            if ((npc.StatusFlags & StatusFlags.InCombat) != 0)
                return true;
        }

        return false;
    }
}
