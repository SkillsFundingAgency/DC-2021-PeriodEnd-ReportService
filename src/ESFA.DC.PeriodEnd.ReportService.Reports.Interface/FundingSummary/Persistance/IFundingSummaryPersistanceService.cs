using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Persistance
{
    public interface IFundingSummaryPersistanceService
    {
        Task PersistAsync(IReportServiceContext reportServiceContext, FundingSummaryReportModel fundingSummaryReportModel, CancellationToken cancellationToken);
    }
}
