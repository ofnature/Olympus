using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Daedalus.Data;
using Daedalus.Rotation.NikeCore.Abilities;
using Daedalus.Rotation.NikeCore.Modules;
using Daedalus.Services.Targeting;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Daedalus.Tests.Rotation.NikeCore.Modules;

/// <summary>
/// Combo finisher at p6 with starter fallback at p7 (PLD/WAR parity).
/// </summary>
public class NikeComboFallbackTests
{
    private readonly DamageModule _module = new();

    [Fact]
    public void StCombo_QueuesGekkoAt6AndHakazeAt7_WhenAfterJinpu()
    {
        var enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<uint>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);
        targeting.Setup(x => x.CountEnemiesInRange(It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(1);

        var config = NikeTestContext.CreateDefaultSamuraiConfiguration();
        config.Samurai.EnableAoERotation = true;
        config.Samurai.AoEMinTargets = 3;

        var scheduler = SchedulerFactory.CreateForTest(config: config);
        var context = NikeTestContext.Create(
            config: config,
            targetingService: targeting,
            level: 90,
            comboStep: 2,
            lastComboAction: SAMActions.Jinpu.ActionId);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == NikeAbilities.Gekko && c.Priority == 6);
        Assert.Contains(gcd, c => c.Behavior == NikeAbilities.Hakaze && c.Priority == 7);
    }

    [Fact]
    public void AoECombo_QueuesMangetsuAt6AndFukoAt7_WhenAfterFuko()
    {
        var enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<uint>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);
        targeting.Setup(x => x.CountEnemiesInRange(It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(3);

        var config = NikeTestContext.CreateDefaultSamuraiConfiguration();
        config.Samurai.EnableAoERotation = true;
        config.Samurai.AoEMinTargets = 3;

        var scheduler = SchedulerFactory.CreateForTest(config: config);
        var context = NikeTestContext.Create(
            config: config,
            targetingService: targeting,
            level: 100,
            comboStep: 1,
            lastComboAction: SAMActions.Fuko.ActionId);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == NikeAbilities.Mangetsu && c.Priority == 6);
        Assert.Contains(gcd, c => c.Behavior == NikeAbilities.Fuko && c.Priority == 7);
    }

    private static Mock<IBattleNpc> CreateMockEnemy(ulong objectId = 99999UL)
    {
        var mock = new Mock<IBattleNpc>();
        mock.Setup(x => x.GameObjectId).Returns(objectId);
        mock.Setup(x => x.CurrentHp).Returns(10000u);
        mock.Setup(x => x.MaxHp).Returns(10000u);
        return mock;
    }
}
