using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface
{
    public interface IReportDataPersistanceService<in T>
    {
        Task PersistAsync(IReportServiceContext reportServiceContext, IEnumerable<T> reportModels,
            CancellationToken cancellationToken);
    }
}
