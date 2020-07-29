using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount
{
    public interface IActCountModelBuilder
    {
        Task<IEnumerable<ActCountModel>> BuildAsync(CancellationToken cancellationToken);
    }
}
