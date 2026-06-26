using Daedalus.Rotation.HermesCore.Context;

namespace Daedalus.Rotation.HermesCore.Helpers;

/// <summary>
/// ABB (always be burning) policy — when burst pooling is off, never stall GCDs/oGCDs for raid alignment.
/// </summary>
internal static class HermesBurnHelper
{
    public static bool ShouldPoolForRaidBurst(IHermesContext context)
        => context.Configuration.Ninja.EnableBurstPooling;
}
