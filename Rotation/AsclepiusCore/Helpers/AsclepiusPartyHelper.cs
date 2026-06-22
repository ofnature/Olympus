using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;
using Olympus.Config;
using Olympus.Data;
using Olympus.Rotation.ApolloCore.Helpers;
using Olympus.Rotation.Common.Helpers;
using Olympus.Services.Party;
using Olympus.Services.Prediction;
using Olympus.Services.Targeting;

namespace Olympus.Rotation.AsclepiusCore.Helpers;

/// <summary>
/// Sage party helper with trust-aware tank resolution and AoE heal counting.
/// </summary>
public class AsclepiusPartyHelper : HealerPartyHelper, IPartyHelper
{
    private readonly AsclepiusStatusHelper _statusHelper;

    public AsclepiusPartyHelper(
        IObjectTable objectTable,
        IPartyList partyList,
        HpPredictionService hpPredictionService,
        Configuration configuration,
        AsclepiusStatusHelper statusHelper)
        : base(objectTable, partyList, hpPredictionService, configuration)
    {
        _statusHelper = statusHelper;
    }

    #region IPartyHelper-style Methods

    public IBattleChara? FindLowestHpPartyMember(IPlayerCharacter player, int healAmount = 0) =>
        FindLowestHpPartyMember(player, SGEActions.Diagnosis.RangeSquared, healAmount);

    public IBattleChara? FindDeadPartyMemberNeedingRaise(IPlayerCharacter player) =>
        FindDeadPartyMemberNeedingRaise(player, RoleActions.Ascend.RangeSquared);

    public (int count, List<uint> targetIds) CountPartyMembersNeedingAoEHeal(IPlayerCharacter player, int healAmount) =>
        CountPartyMembersNeedingAoEHeal(player, SGEActions.Prognosis.RadiusSquared, healAmount);

    (int count, bool anyHaveRegen, List<(uint entityId, string name)> allTargets, int averageMissingHp)
        IPartyHelper.CountPartyMembersNeedingAoEHeal(IPlayerCharacter player, int healAmount)
    {
        var (count, targetIds) = CountPartyMembersNeedingAoEHeal(player, healAmount);
        var allTargets = new List<(uint entityId, string name)>(targetIds.Count);
        foreach (var id in targetIds)
            allTargets.Add((id, string.Empty));
        return (count, false, allTargets, 0);
    }

    (IBattleChara? target, int count, List<uint> targetIds) IPartyHelper.FindBestCureIIITarget(
        IPlayerCharacter player, int healAmount) =>
        (null, 0, []);

    IBattleChara? IPartyHelper.FindRegenTarget(
        IPlayerCharacter player, float tankHpThreshold, float nonTankHpThreshold, float regenRefreshThreshold) =>
        null;

    bool IPartyHelper.NeedsRegen(IBattleChara target, float hpThreshold, float refreshThreshold) => false;

    IBattleChara? IPartyHelper.FindMostEndangeredPartyMember(
        IPlayerCharacter player,
        IDamageIntakeService damageIntakeService,
        int healAmount,
        IDamageTrendService? damageTrendService,
        IShieldTrackingService? shieldTrackingService) =>
        FindMostEndangeredPartyMember(player, damageIntakeService, healAmount, damageTrendService, shieldTrackingService);

    int IPartyHelper.CountInjuredInAoERange(IPlayerCharacter player, float radius, float hpThreshold)
    {
        var count = 0;
        var radiusSquared = radius * radius;
        var playerPos = player.Position;

        foreach (var member in GetAllPartyMembers(player))
        {
            if (member.IsDead)
                continue;
            if (Vector3.DistanceSquared(playerPos, member.Position) > radiusSquared)
                continue;

            if (GetHpPercent(member) < hpThreshold)
                count++;
        }

        return count;
    }

    #endregion

    /// <inheritdoc />
    public override IBattleChara? FindTankInParty(IPlayerCharacter player) =>
        TrustPartyRoleHelper.FindTankInParty(
            player,
            GetAllPartyMembers(player),
            ObjectTable,
            PartyList,
            _statusHelper.HasTankStance);

    /// <summary>
    /// Party health metrics for AoE heal decisions. Injured count respects <see cref="SageConfig.AoEHealCountMode"/>.
    /// </summary>
    public (float avgHpPercent, float lowestHpPercent, int injuredCount) GetAoEHealMetrics(IPlayerCharacter player)
    {
        var (avgHp, lowestHp, _) = CalculatePartyHealthMetrics(player);

        if (Configuration.Sage.AoEHealCountMode != SageAoEHealCountMode.TankCentered)
        {
            var (_, _, injured) = CalculatePartyHealthMetrics(player);
            return (avgHp, lowestHp, injured);
        }

        var anchor = FindTankInParty(player) ?? player;
        var anchorPos = anchor.Position;
        var radiusSquared = SGEActions.Prognosis.RadiusSquared;
        var injuredNearAnchor = 0;

        foreach (var member in GetAllPartyMembers(player))
        {
            if (member.IsDead)
                continue;
            if (Vector3.DistanceSquared(anchorPos, member.Position) > radiusSquared)
                continue;

            if (GetHpPercent(member) < FFXIVConstants.InjuredHpThreshold)
                injuredNearAnchor++;
        }

        return (avgHp, lowestHp, injuredNearAnchor);
    }

    /// <summary>
    /// Resolves enemy count for Dyskrasia based on <see cref="SageConfig.AoEDamageCountMode"/>.
    /// </summary>
    public int CountEnemiesForAoEDamage(
        IPlayerCharacter player,
        float radius,
        ITargetingService targetingService)
    {
        if (Configuration.Sage.AoEDamageCountMode == SageAoEDamageCountMode.TargetCentered)
        {
            var target = targetingService.FindEnemy(
                Configuration.Targeting.EnemyStrategy,
                SGEActions.GetDamageGcdForLevel(player.Level).Range,
                player);

            if (target is IBattleNpc battleTarget)
                return targetingService.CountEnemiesInRangeOfTarget(radius, battleTarget, player);
        }

        return targetingService.CountEnemiesInRange(radius, player);
    }
}
