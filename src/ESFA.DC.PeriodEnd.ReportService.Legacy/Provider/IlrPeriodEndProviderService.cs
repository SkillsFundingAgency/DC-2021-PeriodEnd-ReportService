﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.ReferenceDataService.ILRReferenceData.Model.Interface;
using ESFA.DC.ILR2021.DataStore.EF;
using ESFA.DC.ILR2021.DataStore.EF.Interface;
using ESFA.DC.ILR2021.DataStore.EF.Invalid.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Provider.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using Microsoft.EntityFrameworkCore;
using ReturnPeriod = ESFA.DC.CollectionsManagement.Models.ReturnPeriod;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Provider
{
    public sealed class IlrPeriodEndProviderService : AbstractFundModelProviderService, IIlrPeriodEndProviderService
    {
        private const int ApprenticeshipsFundModel = 36;
        private readonly Func<IIlr2021Context> _ilrContextFactory;
        private readonly Func<IIlr2021Context> _ilrValidContextFactory;
        private readonly Func<IIlr2021InvalidContext> _ilrInValidContextFactory;
        private readonly Func<IIlrReferenceDataContext> _ilrRefDataFactory;

        public IlrPeriodEndProviderService(
            ILogger logger,
            Func<IIlr2021Context> ilrContextFactory,
            Func<IIlr2021Context> ilrValidContextFactory,
            Func<IIlr2021InvalidContext> ilrInValidContextFactory,
            Func<IIlrReferenceDataContext> ilrRefDataFactory)
            : base(logger)
        {
            _ilrContextFactory = ilrContextFactory;
            _ilrValidContextFactory = ilrValidContextFactory;
            _ilrInValidContextFactory = ilrInValidContextFactory;
            _ilrRefDataFactory = ilrRefDataFactory;
        }

        public async Task<IEnumerable<FileDetailModel>> GetFileDetailsAsync(CancellationToken cancellationToken)
        {
            using (var ilrContext = _ilrContextFactory())
            {
                return await ilrContext
                    .FileDetails
                    .Where(x => x.Success == true)
                    .Select(x => new FileDetailModel
                    {
                        Ukprn = x.UKPRN,
                        Filename = x.Filename,
                        SubmittedTime = x.SubmittedTime
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<ProviderSubmissionModel>> GetFileDetailsLatestSubmittedAsync(
            CancellationToken cancellationToken)
        {
            using (var ilrContext = _ilrContextFactory())
            {
                List<long> fd = await ilrContext.FileDetails
                    .Where(x => x.Success == true)
                    .GroupBy(x => x.UKPRN)
                    .Select(x => x.Max(y => y.ID))
                    .ToListAsync(cancellationToken);

                return await ilrContext.FileDetails.Where(x => fd.Contains(x.ID))
                    .Select(x => new ProviderSubmissionModel
                    {
                        Ukprn = x.UKPRN,
                        SubmittedDateTime = x.SubmittedTime.GetValueOrDefault(),
                        TotalErrors = x.TotalErrorCount.GetValueOrDefault(),
                        TotalInvalid = x.TotalInvalidLearnersSubmitted.GetValueOrDefault(),
                        TotalValid = x.TotalValidLearnersSubmitted.GetValueOrDefault(),
                        TotalWarnings = x.TotalWarningCount.GetValueOrDefault()
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<ProviderSubmissionModel> GetFileDetailsLatestSubmittedAsync(long ukPrn, string fileName, int returnPeriod, CancellationToken cancellationToken)
        {
            using (var ilrContext = _ilrContextFactory())
            {
                return await ilrContext.FileDetails
                    .Where(f => f.Filename == fileName)
                    .Select(x => new ProviderSubmissionModel
                    {
                        Ukprn = ukPrn,
                        ReturnPeriod = returnPeriod,
                        SubmittedDateTime = x.SubmittedTime.GetValueOrDefault(),
                        TotalErrors = x.TotalErrorCount.GetValueOrDefault(),
                        TotalInvalid = x.TotalInvalidLearnersSubmitted.GetValueOrDefault(),
                        TotalValid = x.TotalValidLearnersSubmitted.GetValueOrDefault(),
                        TotalWarnings = x.TotalWarningCount.GetValueOrDefault()
                    })
                    .SingleOrDefaultAsync(cancellationToken) ?? new ProviderSubmissionModel
                    {
                        Ukprn = ukPrn,
                        ReturnPeriod = returnPeriod
                    };
            }
        }

        public async Task<IDictionary<string, ProviderSubmissionModel>> GeFileDetailsSubmittedAsync(CancellationToken cancellationToken)
        {
            using (var ilrContext = _ilrContextFactory())
            {
                return await ilrContext.FileDetails
                    .ToDictionaryAsync(x => x.Filename, x => new ProviderSubmissionModel
                    {
                        Ukprn = x.UKPRN,
                        SubmittedDateTime = x.SubmittedTime.GetValueOrDefault(),
                        TotalErrors = x.TotalErrorCount.GetValueOrDefault(),
                        TotalInvalid = x.TotalInvalidLearnersSubmitted.GetValueOrDefault(),
                        TotalValid = x.TotalValidLearnersSubmitted.GetValueOrDefault(),
                        TotalWarnings = x.TotalWarningCount.GetValueOrDefault()
                    }, StringComparer.OrdinalIgnoreCase, cancellationToken);
            }
        }

        public async Task<AppsMonthlyPaymentILRInfo> GetILRInfoForAppsMonthlyPaymentReportAsync(int ukPrn,
            CancellationToken cancellationToken)
        {
            AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo = null;

            try
            {
                appsMonthlyPaymentIlrInfo = new AppsMonthlyPaymentILRInfo()
                {
                    UkPrn = ukPrn,
                    Learners = new List<AppsMonthlyPaymentLearnerModel>()
                };

                cancellationToken.ThrowIfCancellationRequested();

                using (var ilrContext = _ilrValidContextFactory())
                {
                    appsMonthlyPaymentIlrInfo.Learners = await ilrContext.Learners
                        .Where(x => x.UKPRN == ukPrn && x.LearningDeliveries.Any(y => y.FundModel == ApprenticeshipsFundModel))
                        .Select(learner =>
                            new AppsMonthlyPaymentLearnerModel
                            {
                                Ukprn = learner.UKPRN,
                                LearnRefNumber = learner.LearnRefNumber,
                                UniqueLearnerNumber = learner.ULN,
                                FamilyName = learner.FamilyName,
                                GivenNames = learner.GivenNames,
                                CampId = learner.CampId,
                                LearningDeliveries = learner
                                                 .LearningDeliveries
                                                 .Where(ld => ld.FundModel == ApprenticeshipsFundModel)
                                                 .Select(x =>
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
                                                     ProgType = x.ProgType ?? 0,
                                                     StdCode = x.StdCode ?? 0,
                                                     FworkCode = x.FworkCode ?? 0,
                                                     PwayCode = x.PwayCode ?? 0,
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
                                                 }).ToList(),
                                ProviderSpecLearnerMonitorings = learner.ProviderSpecLearnerMonitorings.Select(x =>
                            new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo
                            {
                                Ukprn = x.UKPRN,
                                LearnRefNumber = x.LearnRefNumber,
                                ProvSpecLearnMon = x.ProvSpecLearnMon,
                                ProvSpecLearnMonOccur = x.ProvSpecLearnMonOccur
                            }).ToList(),
                                LearnerEmploymentStatus = learner.LearnerEmploymentStatuses.Select(x =>
                        new AppsMonthlyPaymentLearnerEmploymentStatusInfo
                        {
                            Ukprn = x.UKPRN,
                            LearnRefNumber = x.LearnRefNumber,
                            DateEmpStatApp = x.DateEmpStatApp,
                            EmpStat = x.EmpStat,
                            EmpdId = x.EmpId,
                            AgreeId = string.Empty
                        }).ToList()
                            }).ToListAsync(cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get ILR data", e);
                throw;
            }

            return appsMonthlyPaymentIlrInfo;
        }

        public async Task<List<AppsAdditionalPaymentLearnerInfo>> GetILRInfoForAppsAdditionalPaymentsReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            using (var ilrContext = _ilrValidContextFactory())
            {
                return await ilrContext
                    .Learners
                     .Where(
                        x => x.UKPRN == ukPrn && x.LearningDeliveries.Any(y => y.FundModel == ApprenticeshipsFundModel))
                    .Select(learner =>
                        new AppsAdditionalPaymentLearnerInfo
                        {
                            LearnRefNumber = learner.LearnRefNumber,
                            ULN = learner.ULN,
                            LearningDeliveries = learner.LearningDeliveries.Select(x =>
                                new AppsAdditionalPaymentLearningDeliveryInfo()
                                {
                                    UKPRN = ukPrn,
                                    LearnRefNumber = x.LearnRefNumber,
                                    LearnAimRef = x.LearnAimRef,
                                    AimType = x.AimType,
                                    LearnStartDate = x.LearnStartDate,
                                    ProgType = x.ProgType ?? 0,
                                    StdCode = x.StdCode ?? 0,
                                    FworkCode = x.FworkCode ?? 0,
                                    PwayCode = x.PwayCode ?? 0,
                                    AimSeqNumber = x.AimSeqNumber,
                                    FundModel = x.FundModel
                                }).ToList(),
                            ProviderSpecLearnerMonitorings = learner.ProviderSpecLearnerMonitorings.Select(x =>
                                    new AppsAdditionalPaymentProviderSpecLearnerMonitoringInfo()
                                    {
                                        UKPRN = x.UKPRN,
                                        LearnRefNumber = x.LearnRefNumber,
                                        ProvSpecLearnMon = x.ProvSpecLearnMon,
                                        ProvSpecLearnMonOccur = x.ProvSpecLearnMonOccur
                                    }).ToList()
                        })
                     .ToListAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<DataQualityReturningProviders>> GetReturningProvidersAsync(
            int collectionYear,
            IEnumerable<ReturnPeriod> returnPeriods,
            IEnumerable<FilePeriodInfo> fileDetails,
            CancellationToken cancellationToken)
        {
            List<DataQualityReturningProviders> returningProviders = new List<DataQualityReturningProviders>();
            var fd = fileDetails.ToList();

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
                .GroupBy(x => new {x.PeriodNumber})
                .Select(x => new
                {
                    PeriodNumber = x.Key.PeriodNumber,
                    Collection = $"R{x.Key.PeriodNumber:D2}",
                    Earliest = x.Min(y => y.SubmittedTime ?? DateTime.MaxValue),
                    Latest = x.Max(y => y.SubmittedTime ?? DateTime.MinValue),
                    Providers = x.Select(y => y.UKPRN).Distinct().Count(),
                    Valid = x.Count()
                })
                .OrderByDescending(x => x.PeriodNumber);

            foreach (var f in fdCs)
            {
                returningProviders.Add(new DataQualityReturningProviders
                {
                    Description = "Returning Providers per Period",
                    Collection = f.Collection,
                    NoOfProviders = f.Providers,
                    NoOfValidFilesSubmitted = f.Valid,
                    EarliestValidSubmission = f.Earliest,
                    LastValidSubmission = f.Latest
                });
            }

            return returningProviders;
        }

        public async Task<IEnumerable<RuleViolationsInfo>> GetTop20RuleViolationsAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<RuleViolationsInfo> top20RuleViolationsList;
            using (var ilrContext = _ilrContextFactory())
            using (var ilrReferenceData = _ilrRefDataFactory())
            {
                top20RuleViolationsList = await ilrContext.ValidationErrors
                    .Where(x => x.Severity == "E")
                    .GroupBy(x => new {x.RuleName, x.ErrorMessage})
                    .Select(x => new RuleViolationsInfo
                    {
                        RuleName = x.Key.RuleName,
                        Providers = x.Select(y => y.UKPRN).Distinct().Count(),
                        Learners = x.Select(y => y.LearnRefNumber).Distinct().Count(),
                        NoOfErrors = x.Select(y => y.ErrorMessage).Count()
                    })
                    .OrderByDescending(x => x.NoOfErrors)
                    .ThenBy(x => x.RuleName)
                    .ThenByDescending(x => x.Providers)
                    .Take(20)
                    .ToListAsync(cancellationToken);

                var errors = await ilrReferenceData.Rules
                    .Where(e => top20RuleViolationsList.Exists(rule => rule.RuleName == e.Rulename))
                    .Select(e => new
                    {
                        e.Rulename,
                        e.Message
                    }).ToListAsync(cancellationToken);

                foreach (var ruleViolationsInfo in top20RuleViolationsList)
                {
                    ruleViolationsInfo.ErrorMessage = errors
                        .FirstOrDefault(e => e.Rulename == ruleViolationsInfo.RuleName)?.Message;
                }
            }

            return top20RuleViolationsList;
        }

        public async Task<IEnumerable<ProviderWithoutValidLearners>> GetProvidersWithoutValidLearners(IEnumerable<FilePeriodInfo> fileDetails, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<long> validLearnerUkprns;
            using (var ilrValidContext = _ilrValidContextFactory())
            {
                validLearnerUkprns = await ilrValidContext
                    .Learners
                    .Select(x => (long)x.UKPRN)
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
            IEnumerable<FilePeriodInfo> fileDetails,
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

            List<Top10ProvidersWithInvalidLearnersValidLearners> validLearnersForUkprns;
            List<long> ukPrns = top10ProvidersWithInvalidLearnersInvalid.Select(x => x.Ukprn).ToList();
            using (var ilrContext = _ilrValidContextFactory())
            {
                validLearnersForUkprns = (await ilrContext.Learners
                        .Where(x => ukPrns.Contains(x.UKPRN))
                        .GroupBy(x => x.UKPRN)
                        .ToListAsync(cancellationToken))
                    .Select(x => new Top10ProvidersWithInvalidLearnersValidLearners
                    {
                        Ukprn = x.Key,
                        NoOfValidLearners = x.Count()
                    })
                    .ToList();
            }

            List<Top10ProvidersWithInvalidLearners> fileDetailsForUkprns;
            using (var ilrContext = _ilrContextFactory())
            {
                fileDetailsForUkprns = (await ilrContext.FileDetails
                        .Where(x => ukPrns.Contains(x.UKPRN))
                        .GroupBy(x => x.UKPRN)
                        .Select(x => x.OrderByDescending(y => y.SubmittedTime).First())
                        .ToListAsync(cancellationToken))
                    .Select(x => new Top10ProvidersWithInvalidLearners
                    {
                        Ukprn = x.UKPRN,
                        SubmittedDateTime = x.SubmittedTime.GetValueOrDefault(),
                        LatestFileName = x.Filename,
                        LatestReturn = $"R{GetPeriodReturn(x.SubmittedTime.GetValueOrDefault(), returnPeriods):D2}",
                    })
                    .ToList();
            }

            foreach (Top10ProvidersWithInvalidLearners fileDetailsForUkprn in fileDetailsForUkprns)
            {
                fileDetailsForUkprn.NoOfInvalidLearners =
                    top10ProvidersWithInvalidLearnersInvalid.SingleOrDefault(x => x.Ukprn == fileDetailsForUkprn.Ukprn)?.NoOfInvalidLearners ?? 0;
                fileDetailsForUkprn.NoOfValidLearners =
                    validLearnersForUkprns.SingleOrDefault(x => x.Ukprn == fileDetailsForUkprn.Ukprn)?.NoOfValidLearners ?? 0;
            }

            return fileDetailsForUkprns.OrderByDescending(fileDetail => fileDetail.NoOfInvalidLearners);
        }

        public async Task<AppsCoInvestmentILRInfo> GetILRInfoForAppsCoInvestmentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsCoInvestmentIlrInfo = new AppsCoInvestmentILRInfo
            {
                UkPrn = ukPrn,
                Learners = new List<LearnerInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            using (var ilrContext = _ilrValidContextFactory())
            {
                var learners = await ilrContext
                    .Learners
                    .Where(x => x.UKPRN == ukPrn && x.LearningDeliveries.Any(ld => ld.FundModel == 36 && ld.LearnAimRef == "ZPROG001"))
                    .Select(learner =>
                        new LearnerInfo()
                        {
                            LearnRefNumber = learner.LearnRefNumber,
                            LearningDeliveries = learner
                                .LearningDeliveries
                                .Where(ld => ld.FundModel == 36 && ld.LearnAimRef == "ZPROG001")
                                .Select(x => new LearningDeliveryInfo()
                                {
                                    UKPRN = ukPrn,
                                    LearnRefNumber = x.LearnRefNumber,
                                    LearnAimRef = x.LearnAimRef,
                                    AimType = x.AimType,
                                    AimSeqNumber = x.AimSeqNumber,
                                    LearnStartDate = x.LearnStartDate,
                                    FundModel = x.FundModel,
                                    // Note: Payments default to zero instead of null for no value so we need to cater for that here so that joins work correctly
                                    ProgType = x.ProgType ?? 0,
                                    StdCode = x.StdCode ?? 0,
                                    FworkCode = x.FworkCode ?? 0,
                                    PwayCode = x.PwayCode ?? 0,
                                    SWSupAimId = x.SWSupAimId,
                                    AppFinRecords = x.AppFinRecords
                                        .Where(afr => afr.AFinType == "PMR")
                                        .Select(y => new AppFinRecordInfo()
                                        {
                                            LearnRefNumber = y.LearnRefNumber,
                                            AimSeqNumber = y.AimSeqNumber,
                                            AFinType = y.AFinType,
                                            AFinCode = y.AFinCode,
                                            AFinDate = y.AFinDate,
                                            AFinAmount = y.AFinAmount
                                        }).ToList(),
                                    LearningDeliveryFAMs = x.LearningDeliveryFAMs
                                        .Where(fam => fam.LearnDelFAMType == "LDM")
                                        .Select(y => new ESFA.DC.ILR2021.DataStore.EF.LearningDeliveryFAM()
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
                        }).ToListAsync(cancellationToken);

                appsCoInvestmentIlrInfo.Learners = learners;
            }

            return appsCoInvestmentIlrInfo;
        }

        public async Task<List<AppsCoInvestmentRecordKey>> GetUniqueAppsCoInvestmentRecordKeysAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var context = _ilrValidContextFactory())
            {
                return await context
                    .LearningDeliveries
                    .Where(ld => ld.UKPRN == ukprn && ld.LearnAimRef == "ZPROG001")
                    .GroupBy(ld =>
                    new
                    {
                        ld.LearnRefNumber,
                        ld.LearnStartDate,
                        ProgType = ld.ProgType ?? 0,
                        StdCode = ld.StdCode ?? 0,
                        FworkCode = ld.FworkCode ?? 0,
                        PwayCode = ld.PwayCode ?? 0,
                    })
                    .Select(
                        g =>
                        new AppsCoInvestmentRecordKey(
                            g.Key.LearnRefNumber,
                            g.Key.LearnStartDate,
                            g.Key.ProgType,
                            g.Key.StdCode,
                            g.Key.FworkCode,
                            g.Key.PwayCode))
                    .ToListAsync(cancellationToken);
            }
        }

        public int GetPeriodReturn(DateTime? submittedDateTime, IEnumerable<ReturnPeriod> returnPeriods)
        {
            return !submittedDateTime.HasValue
                ? 0
                : returnPeriods
                      .SingleOrDefault(x =>
                          submittedDateTime >= x.StartDateTimeUtc &&
                          submittedDateTime <= x.EndDateTimeUtc)
                      ?.PeriodNumber ?? 99;
        }
    }
}