using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider
{
    public interface IDasDataProvider
    {
        Task<Dictionary<string, Dictionary<int, Dictionary<int, decimal?[][]>>>> Provide(long ukprn,
            CancellationToken cancellationToken);
    }
}
