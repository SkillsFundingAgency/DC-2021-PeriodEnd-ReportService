using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IFM36PeriodEndProviderService
    {
        Task<List<AECLearningDeliveryInfo>> GetLearningDeliveriesForAppsAdditionalPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>> GetApprenticeshipPriceEpisodesForAppsAdditionalPaymentsReportAsync(int ukprn, CancellationToken cancellationToken);

        Task<AppsMonthlyPaymentRulebaseInfo> GetRulebaseDataForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<AppsCoInvestmentRulebaseInfo> GetFM36DataForAppsCoInvestmentReportAsync(int ukPrn, CancellationToken cancellationToken);
    }
}