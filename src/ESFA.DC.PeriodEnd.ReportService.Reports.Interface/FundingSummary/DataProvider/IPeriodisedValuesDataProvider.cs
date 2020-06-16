using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider
{
    public interface IPeriodisedValuesDataProvider
    {
        Task<Dictionary<string, Dictionary<string, decimal?[][]>>> Provide(long ukprn, CancellationToken cancellationToken);
    }
}
