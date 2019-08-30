using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class JobQueueDataProviderService : IJobQueueDataProviderService
    {
        private readonly Func<IJobQueueDataContext> _jobQueueFactory;

        public JobQueueDataProviderService(Func<IJobQueueDataContext> jobQueueFactory)
        {
            _jobQueueFactory = jobQueueFactory;
        }

        public async Task<IEnumerable<ReturnPeriod>> GetReturnPeriodsAsync(int collectionYear, CancellationToken cancellationToken)
        {
            using (var ctx = _jobQueueFactory())
            {
                return await ctx.ReturnPeriod.Include(x => x.Collection).Where(x => x.Collection.Name == $"ILR{collectionYear}").ToListAsync(cancellationToken);
            }
        }
    }
}
