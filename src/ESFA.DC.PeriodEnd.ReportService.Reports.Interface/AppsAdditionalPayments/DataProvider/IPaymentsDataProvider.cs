using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.DataProvider
{
    public interface IPaymentsDataProvider
    {
        Task<ICollection<Payment>> ProvideAsync(int academicYear, long ukprn, CancellationToken cancellationToken);
    }
}