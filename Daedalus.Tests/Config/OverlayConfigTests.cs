namespace Daedalus.Tests.Config;

public class OverlayConfigTests
{
    [Fact]
    public void DefaultOverlayConfig_IsVisible()
    {
        var config = new Daedalus.Config.OverlayConfig();
        Assert.True(config.IsVisible);
    }

    [Fact]
    public void DefaultOverlayConfig_HasNonZeroPosition()
    {
        var config = new Daedalus.Config.OverlayConfig();
        Assert.Equal(100f, config.X);
        Assert.Equal(100f, config.Y);
    }
}
