using Daedalus.Config;
using Xunit;

namespace Daedalus.Tests.Rotation.NyxCore.Modules;

/// <summary>
/// Shadowstride filler-weave is opt-in: it must default off so the gap-closer isn't woven as melee
/// filler (which darts the tank around the pack and starves other oGCD weaves like Salted Earth).
/// </summary>
public sealed class NyxShadowstrideToggleTests
{
    [Fact]
    public void AutoShadowstride_DefaultsOff()
    {
        Assert.False(new TankConfig().AutoShadowstride);
    }

    [Fact]
    public void EnableShadowstride_StaysOn_ForGapClose()
    {
        // The gap-close use stays enabled by default; only the filler-weave is opt-in.
        Assert.True(new TankConfig().EnableShadowstride);
    }
}
