using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount.DataProvider
{
    public interface IActCountDataProvider
    {
        Task<IEnumerable<ActCountModel>> ProvideAsync(CancellationToken cancellationToken);
    }
}
