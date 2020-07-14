using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface
{
    public interface IReportZipService
    {
        Task CreateZipAsync(string reportFileNameKey, IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
