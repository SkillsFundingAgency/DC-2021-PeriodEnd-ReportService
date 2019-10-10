using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR1920.DataStore.EF;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport.PeriodisedValues;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class
        IlrRulebaseProviderService : AbstractFundModelProviderService // ,Interface.Provider.IFm35PeriodEndProviderService
    {
        private readonly Func<IIlr1920RulebaseContext> _ilrRulebaseContextFactory;

        public IlrRulebaseProviderService(
            ILogger logger,
            Func<IIlr1920RulebaseContext> ilrRulebaseContextFactory)
            : base(logger)
        {
            _ilrRulebaseContextFactory = ilrRulebaseContextFactory;
        }

        public async Task<AppsAdditionalPaymentRulebaseInfo> GetFM36DataForAppsAdditionalPaymentReportAsync(
            int ukPrn,
            CancellationToken cancellationToken)
        {
            var appsAdditionalPaymentRulebaseInfo = new AppsAdditionalPaymentRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECApprenticeshipPriceEpisodePeriodisedValues =
                    new List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>(),
                AECLearningDeliveries = new List<AECLearningDeliveryInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            List<AEC_ApprenticeshipPriceEpisode> aecApprenticeshipPriceEpisodes;
            List<AEC_LearningDelivery> aecLearningDeliveries;

            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                aecApprenticeshipPriceEpisodes = await ilrContext.AEC_ApprenticeshipPriceEpisodes
                    .Include(x => x.AEC_ApprenticeshipPriceEpisode_PeriodisedValues).Where(x => x.UKPRN == ukPrn)
                    .ToListAsync(cancellationToken);
                aecLearningDeliveries = await ilrContext.AEC_LearningDeliveries.Where(x => x.UKPRN == ukPrn)
                    .ToListAsync(cancellationToken);
            }

            foreach (var aecApprenticeshipPriceEpisode in aecApprenticeshipPriceEpisodes)
            {
                foreach (var aecApprenticeshipPriceEpisodePeriodisedValue in aecApprenticeshipPriceEpisode
                    .AEC_ApprenticeshipPriceEpisode_PeriodisedValues)
                {
                    var periodisedValue = new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = aecApprenticeshipPriceEpisodePeriodisedValue.LearnRefNumber,
                        AimSeqNumber = (byte?)aecApprenticeshipPriceEpisode?.PriceEpisodeAimSeqNumber,
                        AttributeName = aecApprenticeshipPriceEpisodePeriodisedValue?.AttributeName,
                        Periods = new[]
                        {
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_1.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_2.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_3.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_4.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_5.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_6.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_7.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_8.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_9.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_10.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_11.GetValueOrDefault(),
                            (decimal?)aecApprenticeshipPriceEpisodePeriodisedValue.Period_12.GetValueOrDefault(),
                        }
                    };
                    appsAdditionalPaymentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues
                        .Add(periodisedValue);
                }
            }

            foreach (var aecLearningDelivery in aecLearningDeliveries)
            {
                var aecLearningDeliveryInfo = new AECLearningDeliveryInfo()
                {
                    UKPRN = ukPrn,
                    LearnRefNumber = aecLearningDelivery.LearnRefNumber,
                    AimSeqNumber = aecLearningDelivery.AimSeqNumber,
                    LearnDelEmpIdFirstAdditionalPaymentThreshold =
                        aecLearningDelivery.LearnDelEmpIdFirstAdditionalPaymentThreshold,
                    LearnDelEmpIdSecondAdditionalPaymentThreshold =
                        aecLearningDelivery.LearnDelEmpIdSecondAdditionalPaymentThreshold
                };

                appsAdditionalPaymentRulebaseInfo.AECLearningDeliveries.Add(aecLearningDeliveryInfo);
            }

            return appsAdditionalPaymentRulebaseInfo;
        }

        public async Task<AppsMonthlyPaymentRulebaseInfo> GetRulebaseDataForAppsMonthlyPaymentReportAsync(
            int ukPrn,
            CancellationToken cancellationToken)
        {
            AppsMonthlyPaymentRulebaseInfo appsMonthlyPaymentRulebaseInfo = null;
            try
            {
                appsMonthlyPaymentRulebaseInfo = new AppsMonthlyPaymentRulebaseInfo()
                {
                    UkPrn = ukPrn,
                    AecApprenticeshipPriceEpisodeInfoList =
                        new List<AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo>(),
                    AecLearningDeliveryInfoList = new List<AppsMonthlyPaymentAECLearningDeliveryInfo>()
                };

                cancellationToken.ThrowIfCancellationRequested();

                using (var ilrContext = _ilrRulebaseContextFactory())
                {
                    appsMonthlyPaymentRulebaseInfo.AecApprenticeshipPriceEpisodeInfoList =
                        await ilrContext?.AEC_ApprenticeshipPriceEpisodes
                            .Where(x => x.UKPRN == ukPrn)
                            .Select(ape => new AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo
                            {
                                Ukprn = ape.UKPRN,
                                LearnRefNumber = ape.LearnRefNumber,
                                PriceEpisodeIdentifier = ape.PriceEpisodeIdentifier,
                                AimSequenceNumber = (byte?)ape.PriceEpisodeAimSeqNumber,
                                EpisodeStartDate = ape.EpisodeStartDate,
                                PriceEpisodeActualEndDate = ape.PriceEpisodeActualEndDate,
                                PriceEpisodeActualEndDateIncEPA = ape.PriceEpisodeActualEndDateIncEPA,
                                PriceEpisodeAgreeId = ape.PriceEpisodeAgreeId
                            }).ToListAsync(cancellationToken);

                    appsMonthlyPaymentRulebaseInfo.AecLearningDeliveryInfoList = await ilrContext.AEC_LearningDeliveries
                        .Where(x => x.UKPRN == ukPrn)
                        .Select(ald => new AppsMonthlyPaymentAECLearningDeliveryInfo
                        {
                            Ukprn = ald.UKPRN,
                            LearnRefNumber = ald.LearnRefNumber,
                            PlannedNumOnProgInstalm = ald.PlannedNumOnProgInstalm,
                        }).ToListAsync(cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get Rulebase data", e);
                throw;
            }

            return appsMonthlyPaymentRulebaseInfo;
        }

        public async Task<FM25LearnerInfo> GetFm25LearnerPeriodisedValuesAsync(int ukprn,
            CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var ilrContext = _ilrRulebaseContextFactory())
                {
                    var learners = await ilrContext.FM25_Learners
                        .Select(fm25Learner =>
                            new FM25Learner
                            {
                                LearnRefNumber = fm25Learner.LearnRefNumber,
                                FundLine = fm25Learner.FundLine,
                                LearnerPeriodisedValues = fm25Learner.FM25_FM35_Learner_PeriodisedValues.Select(pv =>
                                    new LearnerPeriodisedValue
                                    {
                                        LearnRefNumber = pv.LearnRefNumber,
                                        AttributeName = pv.AttributeName,
                                        Period1 = pv.Period_1,
                                        Period2 = pv.Period_2,
                                        Period3 = pv.Period_3,
                                        Period4 = pv.Period_4,
                                        Period5 = pv.Period_5,
                                        Period6 = pv.Period_6,
                                        Period7 = pv.Period_7,
                                        Period8 = pv.Period_8,
                                        Period9 = pv.Period_9,
                                        Period10 = pv.Period_10,
                                        Period11 = pv.Period_11,
                                        Period12 = pv.Period_12
                                    }).ToList(),
                            }).ToListAsync(cancellationToken);

                    return new FM25LearnerInfo()
                    {
                        Ukprn = ukprn,
                        Learners = learners
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get ILR data", e);
                throw;
            }
        }

        public Dictionary<string, Dictionary<string, decimal?[][]>> GetFm25LearnerPeriodisedValues(int ukprn)
        {
            using (var context = _ilrRulebaseContextFactory())
            {
                var fm25LearnerPeriodisedValues = context.FM25_Learners
                                                               .Where(ld => ld.UKPRN == ukprn)
                                                               .GroupBy(ld => ld.FundLine,
                                                                   StringComparer.OrdinalIgnoreCase)
                                                               .ToDictionary(k => k.Key,
                                                                   v => v.SelectMany(ld =>
                                                                           ld.FM35_LearningDelivery_PeriodisedValues)
                                                                       .GroupBy(ldpv => ldpv.AttributeName,
                                                                           StringComparer.OrdinalIgnoreCase)
                                                                       .ToDictionary(k => k.Key, value =>
                                                                               value.Select(pvGroup => new decimal?[]
                                                                               {
                                                                                   pvGroup.Period_1,
                                                                                   pvGroup.Period_2,
                                                                                   pvGroup.Period_3,
                                                                                   pvGroup.Period_4,
                                                                                   pvGroup.Period_5,
                                                                                   pvGroup.Period_6,
                                                                                   pvGroup.Period_7,
                                                                                   pvGroup.Period_8,
                                                                                   pvGroup.Period_9,
                                                                                   pvGroup.Period_10,
                                                                                   pvGroup.Period_11,
                                                                                   pvGroup.Period_12,
                                                                               }).ToArray(),
                                                                           StringComparer.OrdinalIgnoreCase),
                                                                   StringComparer.OrdinalIgnoreCase)
                                                           ?? new Dictionary<string, Dictionary<string, decimal?[][]>>();

                return fm25LearnerPeriodisedValues;
            }
        }


        public Dictionary<string, Dictionary<string, decimal?[][]>> GetFm35LearningDeliveryPeriodisedValues(int ukprn)
        {
            using (var context = _ilrRulebaseContextFactory())
            {
                var fm35LearningDeliveryPeriodisedValues = context.FM35_LearningDeliveries
                                                               .Where(ld => ld.UKPRN == ukprn)
                                                               .GroupBy(ld => ld.FundLine,
                                                                   StringComparer.OrdinalIgnoreCase)
                                                               .ToDictionary(k => k.Key,
                                                                   v => v.SelectMany(ld =>
                                                                           ld.FM35_LearningDelivery_PeriodisedValues)
                                                                       .GroupBy(ldpv => ldpv.AttributeName,
                                                                           StringComparer.OrdinalIgnoreCase)
                                                                       .ToDictionary(k => k.Key, value =>
                                                                               value.Select(pvGroup => new decimal?[]
                                                                               {
                                                                                   pvGroup.Period_1,
                                                                                   pvGroup.Period_2,
                                                                                   pvGroup.Period_3,
                                                                                   pvGroup.Period_4,
                                                                                   pvGroup.Period_5,
                                                                                   pvGroup.Period_6,
                                                                                   pvGroup.Period_7,
                                                                                   pvGroup.Period_8,
                                                                                   pvGroup.Period_9,
                                                                                   pvGroup.Period_10,
                                                                                   pvGroup.Period_11,
                                                                                   pvGroup.Period_12,
                                                                               }).ToArray(),
                                                                           StringComparer.OrdinalIgnoreCase),
                                                                   StringComparer.OrdinalIgnoreCase)
                                                           ?? new Dictionary<string, Dictionary<string, decimal?[][]>>();

                return fm35LearningDeliveryPeriodisedValues;
            }
        }
    }
}