using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IIlrPeriodEndProviderService
    {
        Task<AppsMonthlyPaymentILRInfo> GetILRInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<AppsAdditionalPaymentILRInfo> GetILRInfoForAppsAdditionalPaymentsReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<IEnumerable<DataQualityReturningProviders>> GetReturningProvidersAsync(int collectionYear, List<ReturnPeriod> returnPeriods, CancellationToken cancellationToken);
    }
}
