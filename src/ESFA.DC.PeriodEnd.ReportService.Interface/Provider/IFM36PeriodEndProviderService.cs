using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IFM36PeriodEndProviderService
    {
        Task<AppsAdditionalPaymentRulebaseInfo> GetFM36DataForAppsAdditionalPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<AppsMonthlyPaymentRulebaseInfo> GetRulebaseDataForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<AppsCoInvestmentRulebaseInfo> GetFM36DataForAppsCoInvestmentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<LearnerLevelViewFM36Info> GetFM36DataForLearnerLevelView(int ukPrn, CancellationToken cancellationToken);
    }
}