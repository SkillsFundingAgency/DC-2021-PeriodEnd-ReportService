using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface
{
    public interface IReport
    {
        string ReportTaskName { get; }

        Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
