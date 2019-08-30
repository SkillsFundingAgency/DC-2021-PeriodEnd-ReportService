using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR1920.DataStore.EF;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using ESFA.DC.ILR1920.DataStore.EF.Valid;
using ESFA.DC.ILR1920.DataStore.EF.Valid.Interface;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class IlrPeriodEndProviderService : IIlrPeriodEndProviderService
    {
        private const int ApprentishipsFundModel = 36;
        private readonly Func<IIlr1920ValidContext> _ilrValidContextFactory;
        private readonly Func<IIlr1920RulebaseContext> _ilrContextFactory;

        public IlrPeriodEndProviderService(
            ILogger logger,
            Func<IIlr1920ValidContext> ilrValidContextFactory,
            Func<IIlr1920RulebaseContext> ilrContextFactory)
        {
            _ilrValidContextFactory = ilrValidContextFactory;
            _ilrContextFactory = ilrContextFactory;
        }

        public async Task<AppsMonthlyPaymentILRInfo> GetILRInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsMonthlyPaymentIlrInfo = new AppsMonthlyPaymentILRInfo()
            {
                UkPrn = ukPrn,
                Learners = new List<AppsMonthlyPaymentLearnerInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            List<Learner> learnersList;
            using (var ilrContext = _ilrValidContextFactory())
            {
                learnersList = await ilrContext.Learners
                                                    .Include(x => x.LearningDeliveries).ThenInclude(y => y.LearningDeliveryFAMs)
                                                    .Include(x => x.LearningDeliveries).ThenInclude(y => y.ProviderSpecDeliveryMonitorings)
                                                    .Include(x => x.ProviderSpecLearnerMonitorings)
                                                    .Where(x => x.UKPRN == ukPrn && x.LearningDeliveries.Any(y => y.FundModel == ApprentishipsFundModel))
                                                    .ToListAsync(cancellationToken);
            }

            foreach (var learner in learnersList)
            {
                var learnerInfo = new AppsMonthlyPaymentLearnerInfo
                {
                    LearnRefNumber = learner.LearnRefNumber,
                    CampId = learner.CampId,
                    LearningDeliveries = learner.LearningDeliveries.Select(x => new AppsMonthlyPaymentLearningDeliveryInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = x.LearnRefNumber,
                        LearnAimRef = x.LearnAimRef,
                        AimType = x.AimType,
                        SWSupAimId = x.SWSupAimId,
                        LearnStartDate = x.LearnStartDate,
                        ProgType = x.ProgType,
                        StdCode = x.StdCode,
                        FworkCode = x.FworkCode,
                        PwayCode = x.PwayCode,
                        AimSeqNumber = x.AimSeqNumber,
                        EPAOrganisation = x.EPAOrgID,
                        PartnerUkPrn = x.PartnerUKPRN,
                        ProviderSpecDeliveryMonitorings = x.ProviderSpecDeliveryMonitorings.Select(y => new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                        {
                            UKPRN = y.UKPRN,
                            LearnRefNumber = y.LearnRefNumber,
                            AimSeqNumber = y.AimSeqNumber,
                            ProvSpecDelMon = y.ProvSpecDelMon,
                            ProvSpecDelMonOccur = y.ProvSpecDelMonOccur
                        }).ToList(),
                        LearningDeliveryFams = x.LearningDeliveryFAMs.Select(y => new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                        {
                            UKPRN = y.UKPRN,
                            LearnRefNumber = y.LearnRefNumber,
                            AimSeqNumber = y.AimSeqNumber,
                            LearnDelFAMType = y.LearnDelFAMType,
                            LearnDelFAMCode = y.LearnDelFAMCode
                        }).ToList(),
                    }).ToList(),
                    ProviderSpecLearnerMonitorings = learner.ProviderSpecLearnerMonitorings.Select(x => new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                    {
                        UKPRN = x.UKPRN,
                        LearnRefNumber = x.LearnRefNumber,
                        ProvSpecLearnMon = x.ProvSpecLearnMon,
                        ProvSpecLearnMonOccur = x.ProvSpecLearnMonOccur
                    }).ToList(),
                };

                appsMonthlyPaymentIlrInfo.Learners.Add(learnerInfo);
            }

            return appsMonthlyPaymentIlrInfo;
        }

        public async Task<AppsAdditionalPaymentILRInfo> GetILRInfoForAppsAdditionalPaymentsReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsAdditionalPaymentIlrInfo = new AppsAdditionalPaymentILRInfo()
            {
                UkPrn = ukPrn,
                Learners = new List<AppsAdditionalPaymentLearnerInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            List<Learner> learnersList;
            using (var ilrContext = _ilrValidContextFactory())
            {
                learnersList = await ilrContext.Learners
                                                .Where(x => x.UKPRN == ukPrn && x.LearningDeliveries.Any(y => y.FundModel == ApprentishipsFundModel))
                                                .ToListAsync(cancellationToken);
            }

            foreach (var learner in learnersList)
            {
                var learnerInfo = new AppsAdditionalPaymentLearnerInfo
                {
                    LearnRefNumber = learner.LearnRefNumber,
                    ULN = learner.ULN,
                    LearningDeliveries = learner.LearningDeliveries.Select(x => new AppsAdditionalPaymentLearningDeliveryInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = x.LearnRefNumber,
                        LearnAimRef = x.LearnAimRef,
                        AimType = x.AimType,
                        LearnStartDate = x.LearnStartDate,
                        ProgType = x.ProgType,
                        StdCode = x.StdCode,
                        FworkCode = x.FworkCode,
                        PwayCode = x.PwayCode,
                        AimSeqNumber = x.AimSeqNumber,
                        FundModel = x.FundModel
                    }).ToList(),
                    ProviderSpecLearnerMonitorings = learner.ProviderSpecLearnerMonitorings.Select(x => new AppsAdditionalPaymentProviderSpecLearnerMonitoringInfo()
                    {
                        UKPRN = x.UKPRN,
                        LearnRefNumber = x.LearnRefNumber,
                        ProvSpecLearnMon = x.ProvSpecLearnMon,
                        ProvSpecLearnMonOccur = x.ProvSpecLearnMonOccur
                    }).ToList()
                };
                appsAdditionalPaymentIlrInfo.Learners.Add(learnerInfo);
            }

            return appsAdditionalPaymentIlrInfo;
        }

        public async Task<IEnumerable<DataQualityReturningProviders>> GetReturningProvidersAsync(
            int collectionYear,
            List<ReturnPeriod> returnPeriods,
            CancellationToken cancellationToken)
        {
            List<DataQualityReturningProviders> returningProviders = new List<DataQualityReturningProviders>();
            List<FileDetail> fd;

            using (var ilrContext = _ilrContextFactory())
            {
                fd = await ilrContext.FileDetails.Where(x => x.Success == true).ToListAsync(cancellationToken);
            }

            var fds = fd.GroupBy(x => x.UKPRN)
                .Select(x => new
                {
                    Ukrpn = x.Key,
                    Files = x.Select(y => y.Filename).Count()
                });

            returningProviders.Add(new DataQualityReturningProviders
            {
                Description = "Total Returning Providers",
                NoOfProviders = fds.Count(),
                NoOfValidFilesSubmitted = fd.Count,
                EarliestValidSubmission = null,
                LastValidSubmission = null
            });

            foreach (FileDetail fileDetail in fd)
            {
                fileDetail.ID = returnPeriods.SingleOrDefault(x =>
                                        x.StartDateTimeUtc < fileDetail.SubmittedTime &&
                                        x.EndDateTimeUtc > fileDetail.SubmittedTime)
                                    ?.PeriodNumber ?? 99;
            }

            var fdCs = fd
                .GroupBy(x => new { x.ID })
                .Select(x => new
                {
                    Collection = $"R{x.Key.ID.ToString().PadLeft(2, '0')}",
                    Files = x.Select(y => y.Filename).Count(),
                    Earliest = x.Min(y => y.SubmittedTime ?? DateTime.MaxValue),
                    Latest = x.Max(y => y.SubmittedTime ?? DateTime.MinValue)
                })
                .OrderByDescending(x => x.Latest);

            foreach (var f in fdCs)
            {
                returningProviders.Add(new DataQualityReturningProviders
                {
                    Description = "Returning Providers per Period",
                    Collection = f.Collection,
                    NoOfProviders = fds.Count(),
                    NoOfValidFilesSubmitted = fd.Count,
                    EarliestValidSubmission = f.Earliest,
                    LastValidSubmission = f.Latest
                });
            }

            return returningProviders;
        }
    }
}