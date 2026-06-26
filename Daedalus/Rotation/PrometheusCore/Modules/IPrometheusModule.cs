using Daedalus.Rotation.Common;
using Daedalus.Rotation.PrometheusCore.Context;

namespace Daedalus.Rotation.PrometheusCore.Modules;

/// <summary>
/// Interface for Machinist (Prometheus) rotation modules.
/// </summary>
public interface IPrometheusModule : IRangedDpsRotationModule<IPrometheusContext>
{
}
