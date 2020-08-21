using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.DataProvider
{
    public interface IPaymentsDataProvider
    {
        Task<ICollection<Payment>> GetPaymentsAsync(int ukprn, int academicYear, CancellationToken cancellationToken);

        Task<ICollection<DataLock>> GetDASDataLockAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<HBCPInfo>> GetHBCPInfoAsync(int ukprn, CancellationToken cancellationToken);

        Task<IDictionary<long, string>> GetLegalEntityNameAsync(int ukprn, IEnumerable<long> apprenticeshipIds, CancellationToken cancellationToken);
    }
}
