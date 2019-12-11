using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IDASPaymentsProviderService
    {
        Task<List<DASPaymentInfo>> GetPaymentsInfoForAppsAdditionalPaymentsReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<AppsMonthlyPaymentDASInfo> GetPaymentsInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<AppsMonthlyPaymentDasEarningsInfo> GetEarningsInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<AppsCoInvestmentPaymentsInfo> GetPaymentsInfoForAppsCoInvestmentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<List<AppsCoInvestmentRecordKey>> GetUniqueCombinationsOfKeyFromPaymentsAsync(int ukprn, CancellationToken cancellationToken);

        Task<IDictionary<long, string>> GetLegalEntityNameApprenticeshipIdDictionaryAsync(IEnumerable<long?> apprenticeshipIds, CancellationToken cancellationToken);

        Task<LearnerLevelViewDASDataLockInfo> GetDASDataLockInfoAsync(int ukPrn, CancellationToken cancellationToken);

        Task<LearnerLevelViewHBCPInfo> GetHBCPInfoAsync(int ukPrn, CancellationToken cancellationToken);
    }
}