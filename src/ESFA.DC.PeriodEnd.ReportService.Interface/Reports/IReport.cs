using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Reports
{
    public interface IReport
    {
        string ReportTaskName { get; }

        Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
