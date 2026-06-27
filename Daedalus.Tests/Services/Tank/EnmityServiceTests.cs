using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Moq;
using Daedalus.Services.Tank;
using Xunit;

namespace Daedalus.Tests.Services.Tank;

/// <summary>
/// Targeting-proxy enmity checks. Focused on HasLostAggroToOther, which drives the
/// "don't chase a slipped mob" suppression in the tank out-of-melee branches.
/// </summary>
public sealed class EnmityServiceTests
{
    private const uint PlayerEntityId = 0x1000;

    private static EnmityService CreateService()
    {
        var objectTable = new Mock<IObjectTable>();
        var partyList = new Mock<IPartyList>();
        return new EnmityService(objectTable.Object, partyList.Object);
    }

    private static IBattleChara MobTargeting(ulong targetObjectId)
    {
        var mob = new Mock<IBattleChara>();
        mob.Setup(x => x.TargetObjectId).Returns(targetObjectId);
        return mob.Object;
    }

    [Fact]
    public void HasLostAggroToOther_True_WhenMobTargetsAnotherEntity()
    {
        var service = CreateService();
        var mob = MobTargeting(0x2000); // some other player/NPC
        Assert.True(service.HasLostAggroToOther(mob, PlayerEntityId));
    }

    [Fact]
    public void HasLostAggroToOther_False_WhenMobTargetsUs()
    {
        var service = CreateService();
        var mob = MobTargeting(PlayerEntityId);
        Assert.False(service.HasLostAggroToOther(mob, PlayerEntityId));
    }

    [Fact]
    public void HasLostAggroToOther_False_WhenMobHasNoTarget()
    {
        // Un-aggroed / idle pull: TargetObjectId == 0 must NOT count as lost, so initial
        // dash-to-engage still works.
        var service = CreateService();
        var mob = MobTargeting(0);
        Assert.False(service.HasLostAggroToOther(mob, PlayerEntityId));
    }
}
