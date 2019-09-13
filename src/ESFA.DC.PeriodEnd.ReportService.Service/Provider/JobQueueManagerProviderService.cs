using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class JobQueueManagerProviderService : IJobQueueManagerProviderService
    {
        private readonly Func<IJobQueueDataContext> _jobQueueDataFactory;

        public JobQueueManagerProviderService(
            Func<IJobQueueDataContext> jobQueueDataFactory)
        {
            _jobQueueDataFactory = jobQueueDataFactory;
        }

        public async Task<IEnumerable<long>> GetExpectedReturnersUKPRNsAsync(
            string collectionName,
            int returnPeriod,
            IEnumerable<ReturnPeriod> returnPeriods,
            CancellationToken cancellationToken)
        {
            ReturnPeriod rturnPeriod = returnPeriods.Single(x =>
                    x.PeriodNumber == returnPeriod &&
                    x.CollectionName.Equals(collectionName, StringComparison.OrdinalIgnoreCase));

            using (var jobQueueDataContext = _jobQueueDataFactory())
            {
                return await jobQueueDataContext.OrganisationCollection
                    .Include(x => x.Organisation)
                    .Include(x => x.Collection.Name.Equals(collectionName, StringComparison.OrdinalIgnoreCase))
                    .Where(x => x.StartDateTimeUtc <= rturnPeriod.StartDateTimeUtc &&
                                x.EndDateTimeUtc >= rturnPeriod.EndDateTimeUtc)
                    .Select(x => x.Organisation.Ukprn)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<long>> GetActualReturnersUKPRNsAsync(
            string collectionName,
            int returnPeriod,
            IEnumerable<ReturnPeriod> returnPeriods,
            CancellationToken cancellationToken)
        {
            ReturnPeriod rturnPeriod = returnPeriods.Single(x =>
                    x.PeriodNumber == returnPeriod &&
                    x.CollectionName.Equals(collectionName, StringComparison.OrdinalIgnoreCase));

            using (var jobQueueDataContext = _jobQueueDataFactory())
            {
                return await jobQueueDataContext.FileUploadJobMetaData
                    .Include(x => x.Job)
                    .ThenInclude(x => x.Collection.Name.Equals(collectionName, StringComparison.OrdinalIgnoreCase))
                    .Where(x => x.PeriodNumber == returnPeriod &&
                        x.Job.Ukprn.HasValue)
                    .Select(x => x.Job.Ukprn.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
