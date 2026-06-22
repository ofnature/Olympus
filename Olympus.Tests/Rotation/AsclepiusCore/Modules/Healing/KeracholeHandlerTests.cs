using Moq;
using Olympus.Data;
using Olympus.Rotation.AsclepiusCore.Modules.Healing;
using Olympus.Tests.Mocks;
using Olympus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Olympus.Tests.Rotation.AsclepiusCore.Modules.Healing;

public class KeracholeHandlerTests
{
    private readonly KeracholeHandler _handler = new();

    [Fact]
    public void CollectCandidates_TankEmergency_DoesNotPushKerachole()
    {
        var config = AsclepiusTestContext.CreateDefaultSageConfiguration();
        config.Sage.EnableKerachole = true;
        config.Sage.TaurocholeThreshold = 0.55f;
        config.Healing.OgcdEmergencyThreshold = 0.50f;

        var tank = MockBuilders.CreateMockBattleChara(entityId: 1u, currentHp: 55000, maxHp: 153000);
        tank.Setup(x => x.GameObjectId).Returns(0xDEAD0001ul);

        var partyHelper = new Mock<Olympus.Rotation.ApolloCore.Helpers.IPartyHelper>();
        partyHelper.Setup(p => p.FindTankInParty(It.IsAny<Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>()))
            .Returns(tank.Object);
        partyHelper.Setup(p => p.CalculatePartyHealthMetrics(It.IsAny<Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>()))
            .Returns((avgHpPercent: 0.70f, lowestHpPercent: 0.36f, injuredCount: 3));

        var actionService = MockBuilders.CreateMockActionService(canExecuteOgcd: true);
        actionService.Setup(x => x.IsActionReady(SGEActions.Kerachole.ActionId)).Returns(true);

        var context = AsclepiusTestContext.Create(
            config: config,
            partyHelper: partyHelper,
            actionService: actionService,
            level: 100,
            inCombat: true,
            canExecuteOgcd: true,
            addersgallStacks: 2);

        var scheduler = SchedulerFactory.CreateForTest(actionService);

        _handler.CollectCandidates(context, scheduler, isMoving: false);

        Assert.DoesNotContain(
            scheduler.InspectOgcdQueue(),
            c => c.Behavior.Action.ActionId == SGEActions.Kerachole.ActionId);
        Assert.Equal("Tank low (36%)", context.Debug.KeracholeState);
    }
}
