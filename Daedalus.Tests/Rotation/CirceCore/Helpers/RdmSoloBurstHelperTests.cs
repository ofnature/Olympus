using Moq;
using Daedalus.Config;
using Daedalus.Config.DPS;
using Daedalus.Data;
using Daedalus.Rotation.CirceCore.Context;
using Daedalus.Rotation.CirceCore.Helpers;
using Daedalus.Services;
using Daedalus.Services.Action;
using Xunit;

namespace Daedalus.Tests.Rotation.CirceCore.Helpers;

public class RdmSoloBurstHelperTests
{
    [Fact]
    public void AreBurstCooldownsPaired_BothReady_ReturnsTrue()
    {
        var ctx = MockContext(emboldenReady: true, manaficationReady: true);
        Assert.True(RdmSoloBurstHelper.AreBurstCooldownsPaired(ctx.Object, 5f));
    }

    [Fact]
    public void AreBurstCooldownsPaired_EmboldenReady_ManaficationWithinWindow_ReturnsTrue()
    {
        var ctx = MockContext(emboldenReady: true, manaficationReady: false);
        ctx.Setup(c => c.ActionService.GetCooldownRemaining(RDMActions.Manafication.ActionId)).Returns(3f);
        Assert.True(RdmSoloBurstHelper.AreBurstCooldownsPaired(ctx.Object, 5f));
    }

    [Fact]
    public void ShouldHoldMeleeForSoloBurstChain_WhenManaficationUpButNotEmbolden_ReturnsTrue()
    {
        var burst = new Mock<IBurstWindowService>();
        burst.Setup(b => b.UseSoloBurstFallback).Returns(true);
        var ctx = MockContext(hasManafication: true, hasEmbolden: false);
        Assert.True(RdmSoloBurstHelper.ShouldHoldMeleeForSoloBurstChain(ctx.Object, burst.Object));
    }

    [Fact]
    public void ShouldHoldMeleeForSoloBurstChain_BothBuffsActive_ReturnsFalse()
    {
        var burst = new Mock<IBurstWindowService>();
        burst.Setup(b => b.UseSoloBurstFallback).Returns(true);
        var ctx = MockContext(hasManafication: true, hasEmbolden: true);
        Assert.False(RdmSoloBurstHelper.ShouldHoldMeleeForSoloBurstChain(ctx.Object, burst.Object));
    }

    private static Mock<ICirceContext> MockContext(
        bool emboldenReady = false,
        bool manaficationReady = false,
        bool hasManafication = false,
        bool hasEmbolden = false)
    {
        var config = new Configuration { RedMage = new RedMageConfig() };
        var actions = new Mock<IActionService>();
        actions.Setup(a => a.GetCooldownRemaining(It.IsAny<uint>())).Returns((uint _) => 120f);

        var ctx = new Mock<ICirceContext>();
        ctx.Setup(c => c.Configuration).Returns(config);
        ctx.Setup(c => c.ActionService).Returns(actions.Object);
        ctx.Setup(c => c.EmboldenReady).Returns(emboldenReady);
        ctx.Setup(c => c.ManaficationReady).Returns(manaficationReady);
        ctx.Setup(c => c.HasManafication).Returns(hasManafication);
        ctx.Setup(c => c.HasEmbolden).Returns(hasEmbolden);
        ctx.Setup(c => c.LowerMana).Returns(80);
        return ctx;
    }
}
