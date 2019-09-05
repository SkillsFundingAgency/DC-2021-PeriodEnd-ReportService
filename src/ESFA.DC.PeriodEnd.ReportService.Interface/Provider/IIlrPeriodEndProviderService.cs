using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IIlrPeriodEndProviderService
    {
        Task<AppsMonthlyPaymentILRInfo> GetILRInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<AppsAdditionalPaymentILRInfo> GetILRInfoForAppsAdditionalPaymentsReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<IEnumerable<DataQualityReturningProviders>> GetReturningProvidersAsync(int collectionYear, IEnumerable<ReturnPeriod> returnPeriods, CancellationToken cancellationToken);

        Task<IEnumerable<RuleViolationsInfo>> GetTop20RuleViolationsAsync(CancellationToken cancellationToken);

        Task<IEnumerable<ProviderWithoutValidLearners>> GetProvidersWithoutValidLearners(CancellationToken cancellationToken);

        Task<IEnumerable<Top10ProvidersWithInvalidLearners>> GetProvidersWithInvalidLearners(int collectionYear, IEnumerable<ReturnPeriod> returnPeriods, CancellationToken cancellationToken);

        Task<AppsCoInvestmentILRInfo> GetILRInfoForAppsCoInvestmentReportAsync(int ukPrn, CancellationToken cancellationToken);
    }
}
