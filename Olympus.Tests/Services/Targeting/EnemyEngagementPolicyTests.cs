using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;
using Moq;
using Olympus.Services.Targeting;
using Olympus.Tests.Mocks;
using Xunit;

namespace Olympus.Tests.Services.Targeting;

public sealed class EnemyEngagementPolicyTests
{
    [Fact]
    public void ShouldIncludeEnemy_WhenEnemyHasPersonalInCombatFlag()
    {
        var enemy = CreateEnemy(inCombat: true);
        var player = CreatePlayer(inCombat: false);

        Assert.True(EnemyEngagementPolicy.ShouldIncludeEnemyForTargeting(
            enemy.Object,
            currentTargetId: 0,
            playerEffectivelyInCombat: false,
            relaxEnemyInCombatRequirement: false));
    }

    [Fact]
    public void ShouldIncludeEnemy_WhenHardTargetWithoutRelaxOrPersonalCombat()
    {
        const ulong targetId = 42;
        var enemy = CreateEnemy(inCombat: false, gameObjectId: targetId);
        var player = CreatePlayer(inCombat: false);

        Assert.True(EnemyEngagementPolicy.ShouldIncludeEnemyForTargeting(
            enemy.Object,
            targetId,
            playerEffectivelyInCombat: false,
            relaxEnemyInCombatRequirement: false));
    }

    [Fact]
    public void ShouldExcludeUnclaimedHostile_WhenRelaxDisabledAndNoPersonalFlag()
    {
        var enemy = CreateEnemy(inCombat: false);
        var player = CreatePlayer(inCombat: true);

        Assert.False(EnemyEngagementPolicy.ShouldIncludeEnemyForTargeting(
            enemy.Object,
            currentTargetId: 0,
            playerEffectivelyInCombat: true,
            relaxEnemyInCombatRequirement: false));
    }

    [Fact]
    public void ShouldIncludeUnclaimedHostile_WhenRelaxEnabledAndEffectivelyInCombat()
    {
        var enemy = CreateEnemy(inCombat: false);
        var player = CreatePlayer(inCombat: true);

        Assert.True(EnemyEngagementPolicy.ShouldIncludeEnemyForTargeting(
            enemy.Object,
            currentTargetId: 0,
            playerEffectivelyInCombat: true,
            relaxEnemyInCombatRequirement: true));
    }

    [Fact]
    public void ShouldRelaxAutomatically_WhenPartyCombatAssistAndAllyFighting()
    {
        const uint playerEntityId = 100;
        const uint tankEntityId = 200;

        var player = CreatePlayer(inCombat: false, entityId: playerEntityId);
        var tank = CreateBattleChara(inCombat: true, entityId: tankEntityId);

        var partyMember = new Mock<IPartyMember>();
        partyMember.Setup(x => x.EntityId).Returns(tankEntityId);

        var partyList = new Mock<IPartyList>();
        partyList.Setup(x => x.Length).Returns(2);
        partyList.Setup(x => x.GetEnumerator())
            .Returns(new List<IPartyMember> { partyMember.Object }.GetEnumerator());

        var objectTable = new Mock<IObjectTable>();
        objectTable.Setup(x => x.SearchByEntityId(tankEntityId)).Returns(tank.Object);

        var config = MockBuilders.CreateDefaultConfiguration();
        config.EnableOnPartyInCombat = true;

        Assert.True(EnemyEngagementPolicy.ShouldRelaxEnemyInCombatRequirement(
            config, player.Object, partyList.Object, objectTable.Object));
    }

    private static Mock<IPlayerCharacter> CreatePlayer(bool inCombat, uint entityId = 100)
    {
        var mock = new Mock<IPlayerCharacter>();
        mock.Setup(x => x.EntityId).Returns(entityId);
        mock.Setup(x => x.StatusFlags).Returns(inCombat ? StatusFlags.InCombat : 0);
        return mock;
    }

    private static Mock<IBattleNpc> CreateEnemy(bool inCombat, ulong gameObjectId = 500)
    {
        var mock = new Mock<IBattleNpc>();
        mock.Setup(x => x.GameObjectId).Returns(gameObjectId);
        mock.Setup(x => x.StatusFlags).Returns(inCombat ? StatusFlags.InCombat : 0);
        return mock;
    }

    private static Mock<IBattleChara> CreateBattleChara(bool inCombat, uint entityId)
    {
        var mock = new Mock<IBattleChara>();
        mock.Setup(x => x.EntityId).Returns(entityId);
        mock.Setup(x => x.IsDead).Returns(false);
        mock.Setup(x => x.StatusFlags).Returns(inCombat ? StatusFlags.InCombat : 0);
        return mock;
    }
}
