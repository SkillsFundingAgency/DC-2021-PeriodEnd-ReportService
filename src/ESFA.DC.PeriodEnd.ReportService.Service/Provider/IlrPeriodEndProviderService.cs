using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.ILR1920.DataStore.EF;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using ESFA.DC.ILR1920.DataStore.EF.Invalid.Interface;
using ESFA.DC.ILR1920.DataStore.EF.Valid;
using ESFA.DC.ILR1920.DataStore.EF.Valid.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class IlrPeriodEndProviderService : AbstractFundModelProviderService, IIlrPeriodEndProviderService
    {
        private const int ApprentishipsFundModel = 36;
        private readonly Func<IIlr1920RulebaseContext> _ilrContextFactory;
        private readonly Func<IIlr1920ValidContext> _ilrValidContextFactory;
        private readonly Func<IIlr1920InvalidContext> _ilrInValidContextFactory;

        public IlrPeriodEndProviderService(
            ILogger logger,
            Func<IIlr1920RulebaseContext> ilrContextFactory,
            Func<IIlr1920ValidContext> ilrValidContextFactory,
            Func<IIlr1920InvalidContext> ilrInValidContextFactory)
            : base(logger)
        {
            _ilrContextFactory = ilrContextFactory;
            _ilrValidContextFactory = ilrValidContextFactory;
            _ilrInValidContextFactory = ilrInValidContextFactory;
        }

        public async Task<IEnumerable<FileDetail>> GetFileDetailsAsync(CancellationToken cancellationToken)
        {
            using (var ilrContext = _ilrContextFactory())
            {
                return await ilrContext
                    .FileDetails
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<FileDetail>> GetFileDetailsLatestSubmittedAsync(CancellationToken cancellationToken)
        {
            IEnumerable<FileDetail> fileDetails = await GetFileDetailsAsync(cancellationToken);

            var latestFilesSubmitted = fileDetails
                .Where(x => x.Success == true)
                .GroupBy(x => x.UKPRN)
                .Select(x => x.Max(y => y.ID))
                .ToList();

            return fileDetails
                .Join(latestFilesSubmitted, l => l.ID, f => f, (fd, lfs) => fd)
                .ToList();
        }

        public async Task<AppsMonthlyPaymentILRInfo> GetILRInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo = null;

            appsMonthlyPaymentIlrInfo = new AppsMonthlyPaymentILRInfo()
            {
                UkPrn = ukPrn,
                Learners = new List<AppsMonthlyPaymentLearnerModel>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            List<Learner> learnersList;
            using (var ilrContext = _ilrValidContextFactory())
            {
                learnersList = await ilrContext.Learners
                    .Include(x => x.LearningDeliveries).ThenInclude(y => y.LearningDeliveryFAMs)
                    .Include(x => x.LearningDeliveries).ThenInclude(y => y.ProviderSpecDeliveryMonitorings)
                    .Include(x => x.ProviderSpecLearnerMonitorings)
                    .Where(x => x.UKPRN == ukPrn &&
                                x.LearningDeliveries.Any(y => y.FundModel == ApprentishipsFundModel))
                    .ToListAsync(cancellationToken);
            }

            foreach (var learner in learnersList)
            {
                var learnerInfo = new AppsMonthlyPaymentLearnerModel
                {
                    Ukprn = learner.UKPRN,
                    LearnRefNumber = learner.LearnRefNumber,
                    UniqueLearnerNumber = learner.ULN,
                    CampId = learner.CampId,
                    LearningDeliveries = learner.LearningDeliveries.Select(x =>
                                             new AppsMonthlyPaymentLearningDeliveryModel
                                             {
                                                 Ukprn = x.UKPRN,
                                                 LearnRefNumber = x.LearnRefNumber,
                                                 LearnAimRef = x.LearnAimRef,
                                                 AimType = x.AimType,
                                                 AimSeqNumber = (byte)x.AimSeqNumber,
                                                 LearnStartDate = x.LearnStartDate,
                                                 OrigLearnStartDate = x.OrigLearnStartDate,
                                                 LearnPlanEndDate = x.LearnPlanEndDate,
                                                 FundModel = x.FundModel,
                                                 ProgType = x.ProgType,
                                                 StdCode = x.StdCode,
                                                 FworkCode = x.FworkCode,
                                                 PwayCode = x.PwayCode,
                                                 PartnerUkprn = x.PartnerUKPRN,
                                                 ConRefNumber = x.ConRefNumber,
                                                 EpaOrgId = x.EPAOrgID,
                                                 SwSupAimId = x.SWSupAimId,
                                                 CompStatus = x.CompStatus,
                                                 LearnActEndDate = x.LearnActEndDate,
                                                 Outcome = x.Outcome,
                                                 AchDate = x.AchDate,

                                                 ProviderSpecDeliveryMonitorings = x
                                                     .ProviderSpecDeliveryMonitorings
                                                     .Select(y =>
                                                         new
                                                             AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo
                                                         {
                                                             Ukprn = y.UKPRN,
                                                             LearnRefNumber = y.LearnRefNumber,
                                                             AimSeqNumber = (byte?)y.AimSeqNumber,
                                                             ProvSpecDelMon = y.ProvSpecDelMon,
                                                             ProvSpecDelMonOccur = y.ProvSpecDelMonOccur
                                                         }).ToList(),
                                                 LearningDeliveryFams = x.LearningDeliveryFAMs.Select(y =>
                                                     new AppsMonthlyPaymentLearningDeliveryFAMInfo
                                                     {
                                                         Ukprn = y.UKPRN,
                                                         LearnRefNumber = y.LearnRefNumber,
                                                         AimSeqNumber = (byte?)y.AimSeqNumber,
                                                         LearnDelFAMType = y.LearnDelFAMType,
                                                         LearnDelFAMCode = y.LearnDelFAMCode
                                                     }).ToList(),
                                             }).ToList() ?? new List<AppsMonthlyPaymentLearningDeliveryModel>(),
                    ProviderSpecLearnerMonitorings = learner.ProviderSpecLearnerMonitorings.Select(x =>
                        new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo
                        {
                            Ukprn = x.UKPRN,
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
                    .Include(x => x.LearningDeliveries)
                    .Include(x => x.ProviderSpecLearnerMonitorings)
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
            IEnumerable<ReturnPeriod> returnPeriods,
            IEnumerable<FileDetail> fileDetails,
            CancellationToken cancellationToken)
        {
            List<DataQualityReturningProviders> returningProviders = new List<DataQualityReturningProviders>();
            List<FilePeriodInfo> fd;

            fd = fileDetails
                .Where(x => x.Success == true)
                .Select(f => new FilePeriodInfo()
                {
                    UKPRN = f.UKPRN,
                    Filename = f.Filename,
                    PeriodNumber = GetPeriodReturn(f.SubmittedTime, returnPeriods),
                    SubmittedTime = f.SubmittedTime,
                    Success = f.Success
                }).ToList();

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

            var fdCs = fd
                .GroupBy(x => new { x.PeriodNumber })
                .Select(x => new
                {
                    Collection = x.Key.PeriodNumber == 99 ? string.Empty : $"R{x.Key.PeriodNumber.ToString("D2")}",
                    Files = x.Count(),
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

        public async Task<IEnumerable<RuleViolationsInfo>> GetTop20RuleViolationsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<RuleViolationsInfo> top20RuleViolationsList;
            using (var ilrContext = _ilrContextFactory())
            {
                top20RuleViolationsList = await ilrContext.ValidationErrors
                    .Where(x => x.Severity == "E")
                    .GroupBy(x => new { x.RuleName, x.ErrorMessage })
                    .Select(x => new RuleViolationsInfo
                    {
                        RuleName = x.Key.RuleName,
                        ErrorMessage = x.Key.ErrorMessage,
                        Providers = x.Select(y => y.UKPRN).Distinct().Count(),
                        Learners = x.Select(y => y.LearnRefNumber).Distinct().Count(),
                        NoOfErrors = x.Select(y => y.ErrorMessage).Count()
                    })
                    .OrderByDescending(x => x.NoOfErrors)
                    .ThenBy(x => x.RuleName)
                    .ThenByDescending(x => x.Providers)
                    .Take(20)
                    .ToListAsync(cancellationToken);
            }

            return top20RuleViolationsList;
        }

        public async Task<IEnumerable<ProviderWithoutValidLearners>> GetProvidersWithoutValidLearners(IEnumerable<FileDetail> fileDetails, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<int> validLearnerUkprns;
            using (var ilrValidContext = _ilrValidContextFactory())
            {
                validLearnerUkprns = await ilrValidContext
                    .Learners
                    .Select(x => x.UKPRN)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }

            List<ProviderWithoutValidLearners>
                providersWithoutValidLearnersList = fileDetails
                    .Where(x => !validLearnerUkprns.Contains(x.UKPRN))
                    .GroupBy(x => x.UKPRN)
                    .Select(x => new ProviderWithoutValidLearners
                    {
                        Ukprn = x.Key,
                        LatestFileSubmitted = x.Select(y => y.SubmittedTime ?? DateTime.MinValue).Max()
                    })
                    .OrderBy(x => x.Ukprn)
                    .ToList();

            return providersWithoutValidLearnersList;
        }

        public async Task<IEnumerable<Top10ProvidersWithInvalidLearners>> GetProvidersWithInvalidLearners(
            int collectionYear,
            IEnumerable<ReturnPeriod> returnPeriods,
            IEnumerable<FileDetail> fileDetails,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<Top10ProvidersWithInvalidLearnersInvalidLearners> top10ProvidersWithInvalidLearnersInvalid;
            using (var ilrContext = _ilrInValidContextFactory())
            {
                top10ProvidersWithInvalidLearnersInvalid = (await ilrContext.Learners
                    .GroupBy(x => x.UKPRN)
                    .ToListAsync(cancellationToken))
                    .OrderByDescending(x => x.Count())
                    .Take(10)
                    .Select(x => new Top10ProvidersWithInvalidLearnersInvalidLearners
                    {
                        Ukprn = x.Key,
                        NoOfInvalidLearners = x.Count()
                    })
                    .ToList();
            }

            List<Top10ProvidersWithInvalidLearnersValidLearners> top10ProvidersWithInvalidLearnersValid;
            using (var ilrContext = _ilrValidContextFactory())
            {
                top10ProvidersWithInvalidLearnersValid = (await ilrContext.Learners
                    .Join(top10ProvidersWithInvalidLearnersInvalid, l => l.UKPRN, t => t.Ukprn, (lrn, top) => top)
                    .GroupBy(x => x.Ukprn)
                    .ToListAsync(cancellationToken))
                    .Select(x => new Top10ProvidersWithInvalidLearnersValidLearners
                    {
                        Ukprn = x.Key,
                        NoOfValidLearners = x.Count()
                    })
                    .ToList();
            }

            List<Top10ProvidersWithInvalidLearners> top10ProvidersWithInvalidLearners;

            var fileDetailsforUKPRNs = from p in fileDetails
                                            join ukpr in top10ProvidersWithInvalidLearnersInvalid on p.UKPRN equals ukpr.Ukprn into pukpr
                                            group p by p.UKPRN into op
                                            select new
                                            {
                                                UKPRN = op.Key,
                                                ID = op.Max(x => x.ID)
                                            };

            top10ProvidersWithInvalidLearners = fileDetails
                    .Join(fileDetailsforUKPRNs, fd => fd.ID, f => f.ID, (fDetail, fLatest) => fDetail)
                    .Select(x => new Top10ProvidersWithInvalidLearners
                    {
                        Ukprn = x.UKPRN,
                        SubmittedDateTime = x.SubmittedTime.GetValueOrDefault(),
                        LatestFileName = x.Filename,
                        LatestReturn = $"R{GetPeriodReturn(x.SubmittedTime.GetValueOrDefault(), returnPeriods):D2}",
                    }).ToList();

            foreach (Top10ProvidersWithInvalidLearners top10ProvidersWithInvalidLearner in top10ProvidersWithInvalidLearners)
            {
                top10ProvidersWithInvalidLearner.NoOfInvalidLearners =
                    top10ProvidersWithInvalidLearnersInvalid.SingleOrDefault(x => x.Ukprn == top10ProvidersWithInvalidLearner.Ukprn)?.NoOfInvalidLearners ?? 0;
                top10ProvidersWithInvalidLearner.NoOfValidLearners =
                    top10ProvidersWithInvalidLearnersValid.SingleOrDefault(x => x.Ukprn == top10ProvidersWithInvalidLearner.Ukprn)?.NoOfValidLearners ?? 0;
            }

            return top10ProvidersWithInvalidLearners;
        }

        public async Task<AppsCoInvestmentILRInfo> GetILRInfoForAppsCoInvestmentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsCoInvestmentIlrInfo = new AppsCoInvestmentILRInfo
            {
                UkPrn = ukPrn,
                Learners = new List<LearnerInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            List<Learner> learnersList;
            using (var ilrContext = _ilrValidContextFactory())
            {
                learnersList = await ilrContext.Learners
                                                .Include(x => x.LearningDeliveries).ThenInclude(y => y.AppFinRecords)
                                                .Include(x => x.LearnerEmploymentStatuses)
                                                .Where(x => x.UKPRN == ukPrn &&
                                                            x.LearningDeliveries.Any(y => y.FundModel == ApprentishipsFundModel) &&
                                                            x.LearningDeliveries.Any(y => y.AppFinRecords.Any(z => z.AFinType.Equals(Constants.Generics.PMR) &&
                                                                                                                   z.LearnRefNumber.Equals(x.LearnRefNumber))))
                                                .ToListAsync(cancellationToken);
            }

            foreach (var learner in learnersList)
            {
                var learnerInfo = new LearnerInfo
                {
                    LearnRefNumber = learner.LearnRefNumber,
                    LearningDeliveries = learner.LearningDeliveries.Select(x => new LearningDeliveryInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = x.LearnRefNumber,
                        LearnAimRef = x.LearnAimRef,
                        AimType = x.AimType,
                        AimSeqNumber = x.AimSeqNumber,
                        LearnStartDate = x.LearnStartDate,
                        ProgType = x.ProgType,
                        StdCode = x.StdCode,
                        FworkCode = x.FworkCode,
                        PwayCode = x.PwayCode,
                        SWSupAimId = x.SWSupAimId,
                        AppFinRecords = x.AppFinRecords.Select(y => new AppFinRecordInfo()
                        {
                            LearnRefNumber = y.LearnRefNumber,
                            AimSeqNumber = y.AimSeqNumber,
                            AFinType = y.AFinType,
                            AFinCode = y.AFinCode,
                            AFinDate = y.AFinDate,
                            AFinAmount = y.AFinAmount
                        }).ToList(),
                        LearningDeliveryFAMs = x.LearningDeliveryFAMs.Select(y => new LearningDeliveryFAM()
                        {
                            UKPRN = y.UKPRN,
                            LearnRefNumber = y.LearnRefNumber,
                            AimSeqNumber = y.AimSeqNumber,
                            LearnDelFAMType = y.LearnDelFAMType,
                            LearnDelFAMCode = y.LearnDelFAMCode
                        }).ToList(),
                    }).ToList(),
                    LearnerEmploymentStatus = learner.LearnerEmploymentStatuses.Select(x => new LearnerEmploymentStatusInfo()
                    {
                        LearnRefNumber = x.LearnRefNumber,
                        DateEmpStatApp = x.DateEmpStatApp,
                        EmpId = x.EmpId
                    }).ToList()
                };
                appsCoInvestmentIlrInfo.Learners.Add(learnerInfo);
            }

            return appsCoInvestmentIlrInfo;
        }

        private int GetPeriodReturn(DateTime? submittedDateTime, IEnumerable<ReturnPeriod> returnPeriods)
        {
            return !submittedDateTime.HasValue ? 0 : returnPeriods
                    .SingleOrDefault(x =>
                        submittedDateTime >= x.StartDateTimeUtc &&
                        submittedDateTime <= x.EndDateTimeUtc)
                    ?.PeriodNumber ?? 99;
        }
    }
}