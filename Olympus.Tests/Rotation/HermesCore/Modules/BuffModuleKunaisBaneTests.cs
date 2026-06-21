using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Olympus.Data;
using Olympus.Rotation.Common.Scheduling;
using Olympus.Rotation.HermesCore.Abilities;
using Olympus.Rotation.HermesCore.Context;
using Olympus.Rotation.HermesCore.Modules;
using Olympus.Services;
using Olympus.Services.Action;
using Olympus.Services.Targeting;
using Olympus.Tests.Mocks;
using Olympus.Tests.Rotation.Common.Scheduling;
using Olympus.Tests.Rotation.HermesCore;
using Xunit;

namespace Olympus.Tests.Rotation.HermesCore.Modules;

public class BuffModuleKunaisBaneTests
{
    private readonly BuffModule _module = new();

    [Fact]
    public void CollectCandidates_OutOfMeleeRange_SetsApproachDebugWithoutQueueingKb()
    {
        var enemy = new Mock<IBattleNpc>();
        enemy.Setup(x => x.GameObjectId).Returns(99999UL);

        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), NINActions.KunaisBane.ActionId, It.IsAny<IPlayerCharacter>()))
            .Returns((IBattleNpc?)null);

        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(NINActions.KunaisBane.ActionId)).Returns(true);

        var (scheduler, context) = CreateContext(
            actionService: actionService,
            targetingService: targeting,
            hasSuiton: true,
            level: 92);

        context.Configuration.Ninja.EnableMug = false;
        context.Configuration.Ninja.EnableKassatsu = false;
        context.Configuration.Ninja.EnableTenChiJin = false;
        context.Configuration.Ninja.EnableBunshin = false;
        context.Configuration.Ninja.EnableMeisui = false;
        context.Configuration.Ninja.EnableTenriJindo = false;

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.DoesNotContain(scheduler.InspectOgcdQueue(), c => c.Behavior == HermesAbilities.KunaisBane);
        Assert.Equal("Kunai's Bane ready — out of melee range", context.Debug.BuffState);
    }

    private static (RotationScheduler Scheduler, IHermesContext Context) CreateContext(
        Mock<IActionService>? actionService = null,
        Mock<ITargetingService>? targetingService = null,
        bool hasSuiton = false,
        byte level = 100)
    {
        actionService ??= MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(It.IsAny<uint>())).Returns(true);

        targetingService ??= MockBuilders.CreateMockTargetingService();

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = HermesTestContext.Create(
            actionService: actionService,
            targetingService: targetingService,
            level: level,
            hasSuiton: hasSuiton);

        return (scheduler, context);
    }
}
