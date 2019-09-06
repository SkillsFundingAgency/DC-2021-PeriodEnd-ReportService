using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using ESFA.DC.ILR1920.DataStore.EF.Valid;
using ESFA.DC.ILR1920.DataStore.EF.Valid.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class IlrPeriodEndProviderService : AbstractFundModelProviderService, IIlrPeriodEndProviderService
    {
        private const int ApprentishipsFundModel = 36;
        private readonly Func<IIlr1920ValidContext> _ilrValidContextFactory;
        private readonly Func<IIlr1920RulebaseContext> _ilrContextFactory;

        public IlrPeriodEndProviderService(
            ILogger logger,
            Func<IIlr1920ValidContext> ilrValidContextFactory,
            Func<IIlr1920RulebaseContext> ilrContextFactory)
            : base(logger)
        {
            _ilrValidContextFactory = ilrValidContextFactory;
            _ilrContextFactory = ilrContextFactory;
        }

        public async Task<AppsMonthlyPaymentILRInfo> GetILRInfoForAppsMonthlyPaymentReportAsync(
            int ukPrn,
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
                        Ukprn = learner?.UKPRN,
                        LearnRefNumber = learner?.LearnRefNumber,
                        UniqueLearnerNumber = learner?.ULN,
                        CampId = learner?.CampId,
                        LearningDeliveries = learner?.LearningDeliveries.Select(x =>
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
                                                         ?.ProviderSpecDeliveryMonitorings
                                                         .Select(y =>
                                                             new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo
                                                             {
                                                                 Ukprn = y.UKPRN,
                                                                 LearnRefNumber = y?.LearnRefNumber,
                                                                 AimSeqNumber = (byte)y.AimSeqNumber,
                                                                 ProvSpecDelMon = y?.ProvSpecDelMon,
                                                                 ProvSpecDelMonOccur = y?.ProvSpecDelMonOccur
                                                             }).ToList(),
                                                     LearningDeliveryFams = x.LearningDeliveryFAMs.Select(y =>
                                                         new AppsMonthlyPaymentLearningDeliveryFAMInfo
                                                         {
                                                             Ukprn = y?.UKPRN,
                                                             LearnRefNumber = y?.LearnRefNumber,
                                                             AimSeqNumber = (byte)y.AimSeqNumber,
                                                             LearnDelFAMType = y?.LearnDelFAMType,
                                                             LearnDelFAMCode = y?.LearnDelFAMCode
                                                         }).ToList(),
                                                 }).ToList() ?? new List<AppsMonthlyPaymentLearningDeliveryModel>(),
                        ProviderSpecLearnerMonitorings = learner?.ProviderSpecLearnerMonitorings.Select(x =>
                            new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo
                            {
                                Ukprn = x.UKPRN,
                                LearnRefNumber = x?.LearnRefNumber,
                                ProvSpecLearnMon = x?.ProvSpecLearnMon,
                                ProvSpecLearnMonOccur = x?.ProvSpecLearnMonOccur
                            }).ToList(),
                    };

                    appsMonthlyPaymentIlrInfo.Learners.Add(learnerInfo);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get ILR data", e);
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
    }
}