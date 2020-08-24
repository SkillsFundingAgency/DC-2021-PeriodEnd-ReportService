using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface
{
    public interface IReport
    {
        string ReportTaskName { get; }

        bool IncludeInZip { get; }

        Task<string> GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
