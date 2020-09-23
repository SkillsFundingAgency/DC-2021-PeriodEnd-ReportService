using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.DataProvider
{
    public interface IFcsDataProvider
    {
        Task<ICollection<FcsPayment>> ProvidePaymentsAsync(long ukprn);

        Task<ICollection<FcsAllocation>> ProvideAllocationsAsync(long ukprn);

        Task<ICollection<FcsContractAllocation>> ProviderContractsAsync(long ukprn);
    }
}
