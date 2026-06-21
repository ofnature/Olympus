using Olympus.Rotation.HermesCore.Context;

namespace Olympus.Rotation.HermesCore.Helpers;

/// <summary>
/// ABB (always be burning) policy — when burst pooling is off, never stall GCDs/oGCDs for raid alignment.
/// </summary>
internal static class HermesBurnHelper
{
    public static bool ShouldPoolForRaidBurst(IHermesContext context)
        => context.Configuration.Ninja.EnableBurstPooling;
}
