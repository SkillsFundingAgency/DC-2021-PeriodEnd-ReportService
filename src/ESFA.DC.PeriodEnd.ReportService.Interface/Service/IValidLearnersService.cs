using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IValidLearnersService
    {
        Task<List<string>> GetLearnersAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
