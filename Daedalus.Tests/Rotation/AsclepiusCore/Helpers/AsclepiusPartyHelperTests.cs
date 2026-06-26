using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;
using Moq;
using Daedalus.Config;
using Daedalus.Data;
using Daedalus.Rotation.AsclepiusCore.Helpers;
using Daedalus.Rotation.Common.Helpers;
using Daedalus.Services;
using Daedalus.Services.Party;
using Daedalus.Services.Prediction;
using Daedalus.Tests.Mocks;
using Xunit;

namespace Daedalus.Tests.Rotation.AsclepiusCore.Helpers;

public sealed class AsclepiusPartyHelperTests
{
    [Fact]
    public void FindTankInParty_UsesTankStanceForTrustNpc()
    {
        var config = new Configuration();
        var tank = MockBuilders.CreateMockBattleChara(2, currentHp: 10000, maxHp: 10000);
        var dps = MockBuilders.CreateMockBattleChara(3, currentHp: 10000, maxHp: 10000);
        var player = MockBuilders.CreateMockPlayerCharacter();
        player.Setup(x => x.EntityId).Returns(1u);
        player.Setup(x => x.GameObjectId).Returns(1ul);

        var members = new List<IBattleChara> { player.Object, tank.Object, dps.Object };
        var objectTable = MockBuilders.CreateMockObjectTable();
        var partyList = MockBuilders.CreateMockPartyList(0);
        var hpPrediction = new HpPredictionService(new Mock<ICombatEventService>().Object, config);

        var statusHelper = new AsclepiusStatusHelper();
        var helper = new TestableAsclepiusPartyHelper(
            objectTable.Object,
            partyList.Object,
            hpPrediction,
            config,
            statusHelper,
            members,
            entityId => entityId == 2);

        var found = helper.FindTankInParty(player.Object);

        Assert.NotNull(found);
        Assert.Equal(2u, found!.EntityId);
    }

    [Fact]
    public void GetAoEHealMetrics_TankCentered_CountsInjuredNearTankOnly()
    {
        var config = new Configuration();
        config.Sage.AoEHealCountMode = SageAoEHealCountMode.TankCentered;

        var player = MockBuilders.CreateMockPlayerCharacter(currentHp: 10000, maxHp: 10000);
        player.Setup(x => x.EntityId).Returns(1u);
        player.Setup(x => x.GameObjectId).Returns(1ul);
        player.Setup(x => x.Position).Returns(new Vector3(100f, 0f, 0f));
        var tank = MockBuilders.CreateMockBattleChara(2, currentHp: 8000, maxHp: 10000);
        tank.Setup(x => x.Position).Returns(new Vector3(0f, 0f, 0f));

        var injuredNearTank = MockBuilders.CreateMockBattleChara(3, currentHp: 8000, maxHp: 10000);
        injuredNearTank.Setup(x => x.Position).Returns(new Vector3(5f, 0f, 0f));

        var injuredFar = MockBuilders.CreateMockBattleChara(4, currentHp: 8000, maxHp: 10000);
        injuredFar.Setup(x => x.Position).Returns(new Vector3(50f, 0f, 0f));

        var members = new List<IBattleChara>
        {
            player.Object,
            tank.Object,
            injuredNearTank.Object,
            injuredFar.Object,
        };

        var objectTable = MockBuilders.CreateMockObjectTable();
        var partyList = MockBuilders.CreateMockPartyList(0);
        var hpPrediction = new HpPredictionService(new Mock<ICombatEventService>().Object, config);
        var statusHelper = new AsclepiusStatusHelper();

        var helper = new TestableAsclepiusPartyHelper(
            objectTable.Object,
            partyList.Object,
            hpPrediction,
            config,
            statusHelper,
            members,
            entityId => entityId == 2);

        var (_, _, injuredCount) = helper.GetAoEHealMetrics(player.Object);

        Assert.Equal(2, injuredCount);
    }

    private sealed class TestableAsclepiusPartyHelper : AsclepiusPartyHelper
    {
        private readonly IReadOnlyList<IBattleChara> _members;
        private readonly System.Func<uint, bool> _hasTankStance;

        public TestableAsclepiusPartyHelper(
            IObjectTable objectTable,
            IPartyList partyList,
            HpPredictionService hpPredictionService,
            Configuration configuration,
            AsclepiusStatusHelper statusHelper,
            IReadOnlyList<IBattleChara> members,
            System.Func<uint, bool> hasTankStance)
            : base(objectTable, partyList, hpPredictionService, configuration, statusHelper)
        {
            _members = members;
            _hasTankStance = hasTankStance;
        }

        public override IEnumerable<IBattleChara> GetAllPartyMembers(IPlayerCharacter player, bool includeDead = false)
        {
            foreach (var member in _members)
            {
                if (!includeDead && member.IsDead)
                    continue;
                yield return member;
            }
        }

        public override IBattleChara? FindTankInParty(IPlayerCharacter player) =>
            TrustPartyRoleHelper.FindTankInParty(
                player,
                GetAllPartyMembers(player),
                ObjectTable,
                PartyList,
                bc => _hasTankStance(bc.EntityId));
    }
}
