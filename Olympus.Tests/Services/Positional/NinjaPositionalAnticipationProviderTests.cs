using Olympus.Data;
using Olympus.Services.Positional;
using Olympus.Services.Positional.Navigation;
using Xunit;

namespace Olympus.Tests.Services.Positional;

public class NinjaPositionalAnticipationProviderTests
{
    private readonly NinjaPositionalAnticipationProvider _provider = new();

    [Fact]
    public void GetAnticipatedPositional_AfterGustSlash_KazematoiZero_ReturnsFlankArmorCrush()
    {
        var ctx = BaseContext with
        {
            LastComboAction = NINActions.GustSlash.ActionId,
            PlayerLevel = 100,
            Kazematoi = 0,
        };

        var result = _provider.GetAnticipatedPositional(ctx);

        Assert.NotNull(result);
        Assert.Equal(PositionalType.Flank, result.Value.Required);
        Assert.Equal(NINActions.ArmorCrush.ActionId, result.Value.UpcomingFinisherActionId);
        Assert.Equal(PositionalAnticipationReason.ComboSetup, result.Value.Reason);
    }

    [Fact]
    public void GetAnticipatedPositional_AfterGustSlash_KazematoiTwo_ReturnsRearAeolian()
    {
        var ctx = BaseContext with
        {
            LastComboAction = NINActions.GustSlash.ActionId,
            PlayerLevel = 100,
            Kazematoi = 2,
        };

        var result = _provider.GetAnticipatedPositional(ctx);

        Assert.NotNull(result);
        Assert.Equal(PositionalType.Rear, result.Value.Required);
        Assert.Equal(NINActions.AeolianEdge.ActionId, result.Value.UpcomingFinisherActionId);
    }

    [Fact]
    public void GetAnticipatedPositional_AfterSpinningEdge_KazematoiZero_ReturnsFlank()
    {
        var ctx = BaseContext with
        {
            LastComboAction = NINActions.SpinningEdge.ActionId,
            PlayerLevel = 100,
            Kazematoi = 0,
        };

        var result = _provider.GetAnticipatedPositional(ctx);

        Assert.NotNull(result);
        Assert.Equal(PositionalType.Flank, result.Value.Required);
    }

    [Fact]
    public void GetAnticipatedPositional_TrueNorthActive_ReturnsNull()
    {
        var ctx = BaseContext with
        {
            LastComboAction = NINActions.GustSlash.ActionId,
            PlayerLevel = 100,
            HasTrueNorth = true,
            Kazematoi = 0,
        };

        Assert.Null(_provider.GetAnticipatedPositional(ctx));
    }

    private static PositionalAnticipationContext BaseContext => new(
        LastComboAction: 0,
        PlayerLevel: 100,
        HasTrueNorth: false,
        TargetHasPositionalImmunity: false,
        IsAtRear: false,
        IsAtFlank: false);
}
