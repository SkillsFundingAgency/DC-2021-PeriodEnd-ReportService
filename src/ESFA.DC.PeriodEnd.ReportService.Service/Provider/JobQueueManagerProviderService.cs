using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class JobQueueManagerProviderService : IJobQueueManagerProviderService
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

        public async Task<int> GetCollectionIdAsync(string collectionType, CancellationToken cancellationToken)
        {
            using (var jobQueueDataContext = _jobQueueDataFactory())
            {
                return (await jobQueueDataContext.Collection
                    .Include(x => x.ReturnPeriod)
                    .SingleAsync(x => x.Name == collectionType, cancellationToken)).CollectionId;
            }
        }

        public async Task<IEnumerable<OrganisationCollectionModel>> GetExpectedReturnersUKPRNsAsync(
            int collectionId,
            CancellationToken cancellationToken)
        {
            using (var jobQueueDataContext = _jobQueueDataFactory())
            {
                return await jobQueueDataContext.OrganisationCollection
                    .Include(x => x.Organisation)
                    .Where(x => x.CollectionId == collectionId)
                    .Select(x => new OrganisationCollectionModel
                    {
                        End = x.EndDateTimeUtc,
                        Start = x.StartDateTimeUtc,
                        Ukprn = x.Organisation.Ukprn
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<long>> GetActualReturnersUKPRNsAsync(
            int collectionId,
            int returnPeriod,
            CancellationToken cancellationToken)
        {
            using (var jobQueueDataContext = _jobQueueDataFactory())
            {
                return await jobQueueDataContext.FileUploadJobMetaData
                    .Include(x => x.Job)
                    .ThenInclude(x => x.Collection)
                    .Where(x => x.Job.CollectionId == collectionId
                                && x.PeriodNumber == returnPeriod
                                && x.Job.Ukprn.HasValue
                                && x.Job.Status == JobQueue.Status.Completed)
                    .Select(x => x.Job.Ukprn.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<ProviderReturnPeriod>> GetReturnersAndPeriodsAsync(int collectionId, int returnPeriod, CancellationToken cancellationToken)
        {
            using (var jobQueueDataContext = _jobQueueDataFactory())
            {
                return await jobQueueDataContext.FileUploadJobMetaData
                    .Include(x => x.Job)
                    .ThenInclude(x => x.Collection)
                    .Where(x => x.Job.CollectionId == collectionId
                                && x.Job.Ukprn.HasValue
                                && x.Job.Status == JobQueue.Status.Completed
                                && x.PeriodNumber <= returnPeriod)
                    .GroupBy(d => d.Job.Ukprn)
                    .Select(g => g.OrderByDescending(d => d.Id).First())
                    .Select(s => new ProviderReturnPeriod
                    {
                        Ukprn = s.Job.Ukprn.Value,
                        ReturnPeriod = s.PeriodNumber,
                        FileName = s.FileName
                    }).ToListAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<FilePeriodInfo>> GetFilePeriodInfoForCollection(int collectionId, CancellationToken cancellationToken)
        {
            using (var jobQueueContext = _jobQueueDataFactory())
            {
                return await jobQueueContext.FileUploadJobMetaData
                    .Include(x => x.Job)
                    .ThenInclude(x => x.Collection)
                    .Where(x => x.Job.CollectionId == collectionId
                                && x.Job.Ukprn.HasValue
                                && x.Job.Status == JobQueue.Status.Completed)
                    .Select(s => new FilePeriodInfo
                    {
                        UKPRN = s.Job.Ukprn.Value,
                        Filename = s.FileName,
                        PeriodNumber = s.PeriodNumber,
                        SubmittedTime = s.Job.DateTimeCreatedUtc,
                    }).ToListAsync(cancellationToken);
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
