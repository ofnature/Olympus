using Moq;
using Daedalus.Data;
using Daedalus.Rotation.ThemisCore.Abilities;
using Daedalus.Rotation.ThemisCore.Modules;
using Daedalus.Services.Action;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Daedalus.Tests.Rotation.ThemisCore.Modules;

/// <summary>
/// Sheltron oath-overcap dump: in combat with the Oath Gauge at/over the threshold, Sheltron should
/// weave even when there is no damage-reactive reason (TankCooldownService.ShouldUseShortCooldown false),
/// so passively-regenerated gauge isn't wasted and the physical-mit buff stays up.
/// </summary>
public sealed class ThemisSheltronOvercapTests
{
    private readonly MitigationModule _module = new();

    [Fact]
    public void OvercapDump_WeavesSheltron_WhenOathAtCapAndNoDamageReason()
    {
        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        // ShouldUseShortCooldown defaults to false in the test context — no damage-reactive trigger.
        var context = ThemisTestContext.Create(actionService: actionService, oathGauge: 100);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.Contains(scheduler.InspectOgcdQueue(), c => c.Behavior == ThemisAbilities.Sheltron);
    }

    [Fact]
    public void OvercapDump_DoesNotFire_BelowThreshold()
    {
        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = ThemisTestContext.Create(actionService: actionService, oathGauge: 90); // < 100 default

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.DoesNotContain(scheduler.InspectOgcdQueue(), c => c.Behavior == ThemisAbilities.Sheltron);
    }

    [Fact]
    public void OvercapDump_Respects_DisableToggle()
    {
        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var config = ThemisTestContext.CreateDefaultPaladinConfiguration();
        config.Tank.SheltronOathOvercapDump = false;
        var context = ThemisTestContext.Create(config: config, actionService: actionService, oathGauge: 100);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.DoesNotContain(scheduler.InspectOgcdQueue(), c => c.Behavior == ThemisAbilities.Sheltron);
    }
}
