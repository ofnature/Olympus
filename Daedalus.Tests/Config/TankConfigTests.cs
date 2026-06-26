using System.Text.Json;
using Daedalus.Config;
using Daedalus.Data;
using Xunit;

namespace Daedalus.Tests.Config;

public sealed class TankConfigTests
{
    [Fact]
    public void Constructor_SetsPaladinAndWarriorCompDefaults()
    {
        var tank = new TankConfig();

        Assert.Equal(2, tank.PaladinAoEMinTargetsOverride);
        Assert.Equal(2, tank.WarriorAoEMinTargetsOverride);
        Assert.Null(tank.DarkKnightAoEMinTargetsOverride);
        Assert.Null(tank.GunbreakerAoEMinTargetsOverride);
    }

    [Fact]
    public void Deserialize_MissingOverrideKeys_PreservesConstructorDefaults()
    {
        const string json = """{"Tank":{"AoEMinTargets":3}}""";

        var config = JsonSerializer.Deserialize<Configuration>(json);

        Assert.NotNull(config);
        Assert.Equal(3, config!.Tank.AoEMinTargets);
        Assert.Equal(2, config.Tank.PaladinAoEMinTargetsOverride);
        Assert.Equal(2, config.Tank.WarriorAoEMinTargetsOverride);
        Assert.Null(config.Tank.DarkKnightAoEMinTargetsOverride);
        Assert.Null(config.Tank.GunbreakerAoEMinTargetsOverride);
    }

    [Fact]
    public void Deserialize_ExplicitNullOverride_ClearsPaladinDefault()
    {
        const string json = """{"Tank":{"AoEMinTargets":3,"PaladinAoEMinTargetsOverride":null}}""";

        var config = JsonSerializer.Deserialize<Configuration>(json);

        Assert.NotNull(config);
        Assert.Null(config!.Tank.PaladinAoEMinTargetsOverride);
        Assert.Equal(3, config.Tank.GetEffectiveAoEMinTargets(JobRegistry.Paladin));
    }

    [Fact]
    public void GetEffectiveAoEMinTargets_PaladinDefaultOverride_ReturnsTwo()
    {
        var tank = new TankConfig();

        Assert.Equal(2, tank.GetEffectiveAoEMinTargets(JobRegistry.Paladin));
    }

    [Fact]
    public void GetEffectiveAoEMinTargets_WarriorDefaultOverride_ReturnsTwo()
    {
        var tank = new TankConfig();

        Assert.Equal(2, tank.GetEffectiveAoEMinTargets(JobRegistry.Warrior));
    }

    [Fact]
    public void GetEffectiveAoEMinTargets_DrkNullOverride_InheritsGlobal()
    {
        var tank = new TankConfig { AoEMinTargets = 3 };

        Assert.Null(tank.DarkKnightAoEMinTargetsOverride);
        Assert.Equal(3, tank.GetEffectiveAoEMinTargets(JobRegistry.DarkKnight));
    }

    [Fact]
    public void GetEffectiveAoEMinTargets_GnbNullOverride_InheritsGlobal()
    {
        var tank = new TankConfig { AoEMinTargets = 4 };

        Assert.Null(tank.GunbreakerAoEMinTargetsOverride);
        Assert.Equal(4, tank.GetEffectiveAoEMinTargets(JobRegistry.Gunbreaker));
    }

    [Fact]
    public void GetEffectiveAoEMinTargets_ClearPaladinOverride_InheritsGlobal()
    {
        var tank = new TankConfig { AoEMinTargets = 3, PaladinAoEMinTargetsOverride = null };

        Assert.Equal(3, tank.GetEffectiveAoEMinTargets(JobRegistry.Paladin));
    }

    [Fact]
    public void GetEffectiveAoEMinTargets_CustomPaladinOverride_TakesPrecedence()
    {
        var tank = new TankConfig { AoEMinTargets = 3, PaladinAoEMinTargetsOverride = 4 };

        Assert.Equal(4, tank.GetEffectiveAoEMinTargets(JobRegistry.Paladin));
    }
}
