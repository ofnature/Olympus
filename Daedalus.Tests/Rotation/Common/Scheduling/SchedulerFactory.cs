using Dalamud.Plugin.Services;
using Moq;
using Daedalus.Rotation.Common.Scheduling;
using Daedalus.Services.Action;
using Daedalus.Tests.Mocks;

namespace Daedalus.Tests.Rotation.Common.Scheduling;

public static class SchedulerFactory
{
    public static RotationScheduler CreateForTest(
        Mock<IActionService>? actionService = null,
        Mock<IJobGauges>? jobGauges = null,
        Configuration? config = null)
    {
        actionService ??= MockBuilders.CreateMockActionService();
        jobGauges ??= new Mock<IJobGauges>();
        config ??= new Configuration();
        return new RotationScheduler(actionService.Object, jobGauges.Object, config);
    }
}
