using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider
{
    public interface IFcsDataProvider
    {
        Task<IDictionary<string, string>> Provide(long ukprn, CancellationToken cancellationToken);
    }
}
