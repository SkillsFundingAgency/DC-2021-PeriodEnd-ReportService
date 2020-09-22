using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.DataProvider
{
    public interface IFcsDataProvider
    {
        Task<ICollection<FcsPayment>> ProvidePaymentsAsync(long ukprn);

        Task<ICollection<FcsAllocation>> ProvideAllocationsAsync(long ukprn);

        Task<IDictionary<string, List<string>>> ProviderContractsAsync(long ukprn);
    }
}
