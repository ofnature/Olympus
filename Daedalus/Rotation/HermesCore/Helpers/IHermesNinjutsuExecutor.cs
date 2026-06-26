using Dalamud.Game.ClientState.Objects.Types;
using Daedalus.Rotation.HermesCore.Context;

namespace Daedalus.Rotation.HermesCore.Helpers;

/// <summary>
/// RSR-shaped ninjutsu execution surface. Phase R1: Track 1 (DoTenChiJin) only.
/// </summary>
public interface IHermesNinjutsuExecutor
{
    bool IsTcjStepPending { get; }

    void ResetTcjTrack();

    bool TryExecuteTenChiJin(IHermesContext context, IBattleChara? target, int enemyCount);
}
