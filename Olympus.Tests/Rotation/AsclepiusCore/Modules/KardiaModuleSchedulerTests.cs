using Moq;
using Olympus.Data;
using Olympus.Models.Action;
using Olympus.Rotation.AsclepiusCore.Modules;
using Olympus.Tests.Mocks;
using Olympus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Olympus.Tests.Rotation.AsclepiusCore.Modules;

/// <summary>
/// Kardia dispatches directly (not via scheduler) so the recast gate always runs.
/// </summary>
public class KardiaModuleSchedulerTests
{
    private readonly KardiaModule _module = new();

    [Fact]
    public void CollectCandidates_KardiaNotPlaced_ExecutesKardiaDirectly()
    {
        var config = AsclepiusTestContext.CreateDefaultSageConfiguration();
        config.Sage.AutoKardia = true;

        var tank = MockBuilders.CreateMockBattleChara(entityId: 1u, currentHp: 100000, maxHp: 100000);
        tank.Setup(x => x.GameObjectId).Returns(0xDEAD0001ul);
        var partyHelper = new Mock<Olympus.Rotation.ApolloCore.Helpers.IPartyHelper>();
        partyHelper.Setup(p => p.FindTankInParty(It.IsAny<Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>())).Returns(tank.Object);

        var actionService = MockBuilders.CreateMockActionService(canExecuteOgcd: true);

        var context = AsclepiusTestContext.Create(
            config: config,
            partyHelper: partyHelper,
            actionService: actionService,
            level: 100,
            inCombat: true,
            canExecuteOgcd: true,
            hasKardiaPlaced: false);

        var scheduler = SchedulerFactory.CreateForTest(actionService);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        actionService.Verify(
            x => x.ExecuteOgcd(It.Is<ActionDefinition>(a => a.ActionId == SGEActions.Kardia.ActionId), tank.Object.GameObjectId),
            Times.Once);
        Assert.DoesNotContain(scheduler.InspectOgcdQueue(), c => c.Behavior.Action.ActionId == SGEActions.Kardia.ActionId);
    }

    [Fact]
    public void CollectCandidates_KardiaAlreadyOnTank_DoesNotExecuteKardia()
    {
        var config = AsclepiusTestContext.CreateDefaultSageConfiguration();
        config.Sage.AutoKardia = true;

        var tank = MockBuilders.CreateMockBattleChara(entityId: 1u, currentHp: 100000, maxHp: 100000);
        tank.Setup(x => x.GameObjectId).Returns(0xDEAD0001ul);
        var partyHelper = new Mock<Olympus.Rotation.ApolloCore.Helpers.IPartyHelper>();
        partyHelper.Setup(p => p.FindTankInParty(It.IsAny<Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>())).Returns(tank.Object);

        var actionService = MockBuilders.CreateMockActionService(canExecuteOgcd: true);

        var context = AsclepiusTestContext.Create(
            config: config,
            partyHelper: partyHelper,
            actionService: actionService,
            level: 100,
            inCombat: true,
            canExecuteOgcd: true,
            hasKardiaPlaced: true,
            kardiaTargetId: tank.Object.GameObjectId);

        var scheduler = SchedulerFactory.CreateForTest(actionService);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        actionService.Verify(
            x => x.ExecuteOgcd(It.Is<ActionDefinition>(a => a.ActionId == SGEActions.Kardia.ActionId), It.IsAny<ulong>()),
            Times.Never);
    }

    [Fact]
    public void CollectCandidates_AutoKardiaDisabled_DoesNotExecuteKardia()
    {
        var config = AsclepiusTestContext.CreateDefaultSageConfiguration();
        config.Sage.AutoKardia = false;

        var actionService = MockBuilders.CreateMockActionService(canExecuteOgcd: true);

        var context = AsclepiusTestContext.Create(
            config: config,
            actionService: actionService,
            level: 100,
            inCombat: true,
            canExecuteOgcd: true,
            hasKardiaPlaced: false);

        var scheduler = SchedulerFactory.CreateForTest(actionService);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        actionService.Verify(
            x => x.ExecuteOgcd(It.Is<ActionDefinition>(a => a.ActionId == SGEActions.Kardia.ActionId), It.IsAny<ulong>()),
            Times.Never);
    }
}
