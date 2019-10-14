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
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class FM36PeriodEndProviderService : AbstractFundModelProviderService, IFM36PeriodEndProviderService
    {
        private readonly Func<IIlr1920RulebaseContext> _ilrRulebaseContextFactory;

        public FM36PeriodEndProviderService(
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
                            AimSequenceNumber = (byte?)ald.AimSeqNumber,
                            LearnAimRef = ald.LearnAimRef,
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

        public async Task<AppsCoInvestmentRulebaseInfo> GetFM36DataForAppsCoInvestmentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsCoInvestmentRulebaseInfo = new AppsCoInvestmentRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECLearningDeliveries = new List<AECLearningDeliveryInfo>(),
                AECApprenticeshipPriceEpisodePeriodisedValues = new List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                appsCoInvestmentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues = await ilrContext
                    .AEC_ApprenticeshipPriceEpisode_PeriodisedValues
                    .Where(x => x.UKPRN == ukPrn && x.AttributeName == Generics.Fm36PriceEpisodeCompletionPaymentAttributeName)
                    .Select(p => new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = p.LearnRefNumber,
                        AimSeqNumber = p.AEC_ApprenticeshipPriceEpisode.PriceEpisodeAimSeqNumber ?? 0,
                        AttributeName = p.AttributeName,
                        Periods = new[]
                        {
                            p.Period_1,
                            p.Period_2,
                            p.Period_3,
                            p.Period_4,
                            p.Period_5,
                            p.Period_6,
                            p.Period_7,
                            p.Period_8,
                            p.Period_9,
                            p.Period_10,
                            p.Period_11,
                            p.Period_12
                        }
                    }).ToListAsync(cancellationToken);

                appsCoInvestmentRulebaseInfo.AECLearningDeliveries = await ilrContext
                    .AEC_LearningDeliveries
                    .Where(x => x.UKPRN == ukPrn)
                    .Select(ld => new AECLearningDeliveryInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = ld.LearnRefNumber,
                        AimSeqNumber = ld.AimSeqNumber,
                        AppAdjLearnStartDate = ld.AppAdjLearnStartDate,
                    }).ToListAsync(cancellationToken);
            }

            return appsCoInvestmentRulebaseInfo;
        }
    }
}