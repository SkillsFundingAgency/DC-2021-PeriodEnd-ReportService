using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Reports
{
    public interface IInternalReport
    {
        string ReportTaskName { get; }

        string ReportFileName { get; set; }

        Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);

        bool IsMatch(string reportTaskName);
    }
}