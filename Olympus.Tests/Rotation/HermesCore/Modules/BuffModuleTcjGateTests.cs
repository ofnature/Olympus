using Moq;
using Olympus.Data;
using Olympus.Rotation.HermesCore.Abilities;
using Olympus.Rotation.HermesCore.Context;
using Olympus.Rotation.HermesCore.Helpers;
using Olympus.Rotation.HermesCore.Modules;
using Olympus.Services;
using Olympus.Tests.Mocks;
using Olympus.Tests.Rotation.Common.Scheduling;
using Olympus.Tests.Rotation.HermesCore;
using Xunit;

namespace Olympus.Tests.Rotation.HermesCore.Modules;

public class BuffModuleTcjGateTests
{
    private readonly BuffModule _module = new();

    [Fact]
    public void CollectCandidates_OutsideBurstWindow_DoesNotPushTenChiJin()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(It.IsAny<uint>())).Returns(true);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = HermesTestContext.Create(
            actionService: actionService,
            inTrickAttack: false,
            inCombat: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.DoesNotContain(scheduler.InspectOgcdQueue(), c => c.Behavior == HermesAbilities.TenChiJin);
    }

    [Fact]
    public void CollectCandidates_InTrickAttack_PushesTenChiJin()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(It.IsAny<uint>())).Returns(true);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = HermesTestContext.Create(
            actionService: actionService,
            inTrickAttack: true,
            inCombat: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.Contains(scheduler.InspectOgcdQueue(), c => c.Behavior == HermesAbilities.TenChiJin);
    }
}
