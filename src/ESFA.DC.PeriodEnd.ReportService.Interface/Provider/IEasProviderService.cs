using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IEasProviderService
    {
        Task<ProviderEasSubmissionInfo> GetProviderEasSubmissions(int ukPrn, CancellationToken cancellationToken);
    }
}