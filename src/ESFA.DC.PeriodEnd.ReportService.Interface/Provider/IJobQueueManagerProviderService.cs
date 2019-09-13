using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IJobQueueManagerProviderService
    {
        Task<IEnumerable<long>> GetExpectedReturnersUKPRNsAsync(
            string collectionName,
            int returnPeriod,
            IEnumerable<ReturnPeriod> returnPeriods,
            CancellationToken cancellationToken);

        Task<IEnumerable<long>> GetActualReturnersUKPRNsAsync(
            string collectionName,
            int returnPeriod,
            IEnumerable<ReturnPeriod> returnPeriods,
            CancellationToken cancellationToken);
    }
}
