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
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class JobQueueManagerProviderService : IJobQueueManagerProviderService
    {
        private readonly Func<IJobQueueDataContext> _jobQueueDataFactory;

        private readonly string[] _collectionTypes =
        {
            JobQueue.CollectionTypes.ILR,
            JobQueue.CollectionTypes.ESF,
            JobQueue.CollectionTypes.EAS
        };

        private readonly int[] _jobStatuses =
        {
            JobQueue.Status.Completed,
            JobQueue.Status.Failed,
            JobQueue.Status.FailedRetry
        };

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
            ReturnPeriod returnPeriodForCollection = returnPeriods.Single(x =>
                    x.PeriodNumber == returnPeriod &&
                    x.CollectionName.Equals(collectionName, StringComparison.OrdinalIgnoreCase));

            using (var jobQueueDataContext = _jobQueueDataFactory())
            {
                return await jobQueueDataContext.OrganisationCollection
                    .Include(x => x.Organisation)
                    .Include(x => x.Collection)
                    .Where(x => x.Collection.Name.Equals(collectionName, StringComparison.OrdinalIgnoreCase) &&
                                x.StartDateTimeUtc <= returnPeriodForCollection.StartDateTimeUtc &&
                                x.EndDateTimeUtc >= returnPeriodForCollection.EndDateTimeUtc)
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
            using (var jobQueueDataContext = _jobQueueDataFactory())
            {
                return await jobQueueDataContext.FileUploadJobMetaData
                    .Include(x => x.Job)
                    .ThenInclude(x => x.Collection)
                    .Where(x => x.Job.Collection.Name.Equals(collectionName, StringComparison.OrdinalIgnoreCase) &&
                        x.PeriodNumber == returnPeriod &&
                        x.Job.Ukprn.HasValue)
                    .Select(x => x.Job.Ukprn.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<CollectionStatsModel>> GetCollectionStatsModels(
            int collectionYear,
            int collectionPeriod,
            CancellationToken cancellationToken)
        {
            List<CollectionStatsModel> models;

            using (var ctx = _jobQueueDataFactory())
            {
                models = await ctx.Job
                    .Include(x => x.Collection)
                    .ThenInclude(x => x.CollectionType)
                    .Include(x => x.FileUploadJobMetaData)
                    .Where(x => x.Collection.CollectionYear == collectionYear
                                && x.FileUploadJobMetaData.Single().PeriodNumber == collectionPeriod
                                && _collectionTypes.Contains(x.Collection.CollectionType.Type, StringComparer.OrdinalIgnoreCase)
                                && _jobStatuses.Contains(x.Status))
                    .GroupBy(x => x.Collection.Name)
                    .Select(x => new CollectionStatsModel
                    {
                        CollectionName = x.Key,
                        CountOfComplete = x.Count(y => y.Status == JobQueue.Status.Completed),
                        CountOfFail = x.Count(y => y.Status == JobQueue.Status.FailedRetry || y.Status == JobQueue.Status.Failed)
                    })
                    .ToListAsync(cancellationToken);
            }

            models.Add(new CollectionStatsModel
            {
                CollectionName = "Total",
                CountOfComplete = models.Sum(x => x.CountOfComplete),
                CountOfFail = models.Sum(x => x.CountOfFail)
            });

            return models;
        }
    }
}
