using Daedalus.Rotation.ApolloCore.Helpers;
using Daedalus.Rotation.AsclepiusCore.Helpers;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace Daedalus.Rotation.AsclepiusCore.Helpers;

/// <summary>
/// AoE heal metric helpers for Sage handlers.
/// </summary>
public static class AsclepiusPartyMetrics
{
    public static (float avgHpPercent, float lowestHpPercent, int injuredCount) GetAoEHealMetrics(
        IPartyHelper partyHelper,
        IPlayerCharacter player)
    {
        if (partyHelper is AsclepiusPartyHelper sageParty)
            return sageParty.GetAoEHealMetrics(player);

        return partyHelper.CalculatePartyHealthMetrics(player);
    }
}
