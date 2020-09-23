using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.DataProvider
{
    public interface IAppsDataProvider
    {
        Task<ICollection<AppsPayment>> ProvidePaymentsAsync(long ukprn);

        Task<ICollection<AppsAdjustmentPayment>> ProvideAdjustmentPaymentsAsync(long ukprn);
    }
}
