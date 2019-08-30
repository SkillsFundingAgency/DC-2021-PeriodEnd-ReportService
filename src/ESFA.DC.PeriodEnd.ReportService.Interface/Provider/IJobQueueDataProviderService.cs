using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobQueueManager.Data.Entities;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IJobQueueDataProviderService
    {
        Task<IEnumerable<ReturnPeriod>> GetReturnPeriodsAsync(int collectionYear, CancellationToken cancellationToken);
    }
}
