using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Daedalus.Compat;
using Daedalus.Services.Targeting;
using Xunit;

namespace Daedalus.Tests.Services.Targeting;

public sealed class EnemyAttackabilityTests
{
    [Theory]
    [InlineData(BattleNpcKinds.Pet)]
    [InlineData(BattleNpcKinds.Chocobo)]
    [InlineData(BattleNpcKinds.NpcPartyMember)]
    public void IsExcludedBattleNpcKind_ExcludesCompanionTypes(byte kind)
    {
        var npc = new Mock<IBattleNpc>();
        npc.Setup(x => x.BattleNpcKind).Returns((BattleNpcSubKind)kind);

        Assert.True(EnemyAttackability.IsExcludedBattleNpcKind(npc.Object));
    }

    [Fact]
    public void IsExcludedBattleNpcKind_DoesNotExcludeCombatants()
    {
        var npc = new Mock<IBattleNpc>();
        npc.Setup(x => x.BattleNpcKind).Returns((BattleNpcSubKind)BattleNpcKinds.Combatant);

        Assert.False(EnemyAttackability.IsExcludedBattleNpcKind(npc.Object));
    }

    [Fact]
    public void IsPlayerAttackable_ReturnsFalseForExcludedKinds()
    {
        var npc = new Mock<IBattleNpc>();
        npc.Setup(x => x.BattleNpcKind).Returns(BattleNpcSubKind.Pet);
        npc.Setup(x => x.IsTargetable).Returns(true);
        npc.Setup(x => x.IsDead).Returns(false);

        Assert.False(EnemyAttackability.IsPlayerAttackable(npc.Object));
    }

    [Fact]
    public void IsPlayerAttackable_ReturnsFalseForNonBattleNpc()
    {
        var obj = new Mock<IGameObject>();
        Assert.False(EnemyAttackability.IsPlayerAttackable(obj.Object));
    }
}
