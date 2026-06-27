using System.Linq;
using Daedalus.Data;
using Daedalus.Rotation.AresCore.Abilities;
using Xunit;

namespace Daedalus.Tests.Rotation.AresCore.Modules;

/// <summary>
/// Regression: Ares defensive behaviors must carry their level upgrades so the scheduler resolves,
/// gates, and dispatches the level-appropriate id. Without these, the scheduler dispatched the base id
/// (e.g. Vengeance 44) and the base-id quest-unlock gate rejected it at L92+ — the cooldown queued but
/// never fired.
/// </summary>
public sealed class AresAbilityUpgradeTests
{
    [Fact]
    public void Vengeance_UpgradesToDamnationAt92()
    {
        var ups = AresAbilities.Vengeance.LevelReplacements;
        Assert.NotNull(ups);
        Assert.Contains(ups!, u => u.Level == 92 && u.Replacement.ActionId == WARActions.Damnation.ActionId);
    }

    [Fact]
    public void RawIntuition_UpgradesToBloodwhettingAt82()
    {
        var ups = AresAbilities.RawIntuition.LevelReplacements;
        Assert.NotNull(ups);
        Assert.Contains(ups!, u => u.Level == 82 && u.Replacement.ActionId == WARActions.Bloodwhetting.ActionId);
    }
}
