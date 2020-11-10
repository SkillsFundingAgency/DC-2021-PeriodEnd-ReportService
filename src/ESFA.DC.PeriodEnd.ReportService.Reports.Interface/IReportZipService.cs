using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface
{
    public interface IReportZipService
    {
        Task CreateOrUpdateZipWithReportAsync(string zipName, string reportFileNameKey, IReportServiceContext reportServiceContext, CancellationToken cancellationToken);

        Task CreateOrUpdateZipWithReportAsync(string reportFileNameKey, IReportServiceContext reportServiceContext, CancellationToken cancellationToken);

        Task RemoveZipAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}