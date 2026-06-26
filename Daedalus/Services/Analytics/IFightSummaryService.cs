using System;
using System.Collections.Generic;
using Daedalus.Models;

namespace Daedalus.Services.Analytics;

public interface IFightSummaryService
{
    event Action<FightSummaryRecord>? OnSummaryReady;
    IReadOnlyList<FightSummaryRecord> RecentSummaries { get; }
}
