using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Reports
{
    public interface IInternalReport
    {
        Task GenerateReport(CancellationToken cancellationToken);
    }
}