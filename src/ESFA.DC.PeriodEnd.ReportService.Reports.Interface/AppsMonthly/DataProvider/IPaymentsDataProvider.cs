using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.DataProvider
{
    public interface IPaymentsDataProvider
    {
        Task<ICollection<Payment>> GetPaymentsAsync(int ukprn, int academicYear, CancellationToken cancellationToken);

        Task<ICollection<Earning>> GetEarningsAsync(int ukprn, CancellationToken cancellationToken);
    }
}
