using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IFM36PeriodEndProviderService
    {
        Task<AppsAdditionalPaymentRulebaseInfo> GetFM36DataForAppsAdditionalPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<AppsMonthlyPaymentRulebaseInfo> GetFM36DataForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);
    }
}