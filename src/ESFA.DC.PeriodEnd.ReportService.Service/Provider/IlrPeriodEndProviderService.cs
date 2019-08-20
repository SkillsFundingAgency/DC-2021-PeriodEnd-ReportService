﻿using System;
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

        public async Task<AppsMonthlyPaymentILRInfo> GetILRInfoForAppsMonthlyPaymentReportAsync(
            int ukPrn,
            CancellationToken cancellationToken)
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

            // Convert the database null values to a default value so that we don't have to keep checking for null later
            // and to stop exceptions where we're not able to check for null e.g. in LINQ statements
            // Also give 'not null' columns a default value as the table definition may change at a later date causing our code to break
            foreach (var learner in learnersList)
            {
                var learnerInfo = new AppsMonthlyPaymentLearnerInfo
                {
                    Ukprn = learner?.UKPRN.ToString() ?? string.Empty,
                    LearnRefNumber = learner?.LearnRefNumber ?? string.Empty,
                    UniqueLearnerNumber = learner?.ULN.ToString() ?? string.Empty,
                    CampId = learner?.CampId ?? string.Empty,
                    LearningDeliveries = learner?.LearningDeliveries.Select(x => new AppsMonthlyPaymentLearningDeliveryInfo
                    {
                        UKPRN = ukPrn.ToString(),
                        LearnRefNumber = x?.LearnRefNumber ?? string.Empty,
                        LearnAimRef = x?.LearnAimRef ?? string.Empty,
                        AimType = x?.AimType.ToString() ?? string.Empty,
                        SWSupAimId = x?.SWSupAimId ?? string.Empty,
                        LearnStartDate = x?.LearnStartDate.ToString("yyyy-mm-dd") ?? string.Empty,
                        ProgType = x?.ProgType.ToString() ?? string.Empty,
                        StdCode = x?.StdCode.ToString() ?? string.Empty,
                        FworkCode = x?.FworkCode.ToString() ?? string.Empty,
                        PwayCode = x?.PwayCode.ToString() ?? string.Empty,
                        AimSeqNumber = x.AimSeqNumber.ToString() ?? string.Empty,
                        EPAOrganisation = x.EPAOrgID ?? string.Empty,
                        PartnerUkPrn = x?.PartnerUKPRN.ToString() ?? string.Empty,
                        ProviderSpecDeliveryMonitorings = x?.ProviderSpecDeliveryMonitorings.Select(y => new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo
                        {
                            UKPRN = y.UKPRN,
                            LearnRefNumber = y?.LearnRefNumber ?? string.Empty,
                            AimSeqNumber = y.AimSeqNumber,
                            ProvSpecDelMon = y?.ProvSpecDelMon ?? string.Empty,
                            ProvSpecDelMonOccur = y?.ProvSpecDelMonOccur ?? string.Empty
                        }).ToList(),
                        LearningDeliveryFams = x.LearningDeliveryFAMs.Select(y => new AppsMonthlyPaymentLearningDeliveryFAMInfo
                        {
                            UKPRN = y.UKPRN,
                            LearnRefNumber = y?.LearnRefNumber ?? string.Empty,
                            AimSeqNumber = y.AimSeqNumber,
                            LearnDelFAMType = y?.LearnDelFAMType ?? string.Empty,
                            LearnDelFAMCode = y?.LearnDelFAMCode ?? string.Empty
                        }).ToList(),
                    }).ToList() ?? new List<AppsMonthlyPaymentLearningDeliveryInfo>(),
                    ProviderSpecLearnerMonitorings = learner?.ProviderSpecLearnerMonitorings.Select(x =>
                    new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo
                    {
                        UKPRN = x.UKPRN,
                        LearnRefNumber = x?.LearnRefNumber ?? string.Empty,
                        ProvSpecLearnMon = x?.ProvSpecLearnMon ?? string.Empty,
                        ProvSpecLearnMonOccur = x?.ProvSpecLearnMonOccur ?? string.Empty
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
    }
}