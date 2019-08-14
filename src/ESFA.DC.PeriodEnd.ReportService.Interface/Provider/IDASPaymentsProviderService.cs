using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IDASPaymentsProviderService
    {
        Task<AppsAdditionalPaymentDasPaymentsInfo> GetPaymentsInfoForAppsAdditionalPaymentsReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<AppsMonthlyPaymentDASInfo> GetPaymentsInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        //Task<AppsMonthlyPaymentDASInfo> Get1920DasPaymentsAsync(int ukPrn, CancellationToken cancellationToken);
    }
}
