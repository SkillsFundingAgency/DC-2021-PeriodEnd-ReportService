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
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class RulebaseProviderService : AbstractFundModelProviderService, Interface.Provider.IRulebaseProviderService
    {
        private readonly Func<IIlr1920RulebaseContext> _ilrRulebaseContextFactory;

        public RulebaseProviderService(
            ILogger logger,
            Func<IIlr1920RulebaseContext> ilrRulebaseContextFactory)
        : base(logger)
        {
            _ilrRulebaseContextFactory = ilrRulebaseContextFactory;
        }

        public async Task<AppsAdditionalPaymentRulebaseInfo> GetFM36DataForAppsAdditionalPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsAdditionalPaymentRulebaseInfo = new AppsAdditionalPaymentRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECApprenticeshipPriceEpisodePeriodisedValues = new List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>(),
                AECLearningDeliveries = new List<AECLearningDeliveryInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            List<AEC_ApprenticeshipPriceEpisode> aecApprenticeshipPriceEpisodes;
            List<AEC_LearningDelivery> aecLearningDeliveries;

            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                aecApprenticeshipPriceEpisodes = await ilrContext.AEC_ApprenticeshipPriceEpisodes.Include(x => x.AEC_ApprenticeshipPriceEpisode_PeriodisedValues).Where(x => x.UKPRN == ukPrn).ToListAsync(cancellationToken);
                aecLearningDeliveries = await ilrContext.AEC_LearningDeliveries.Where(x => x.UKPRN == ukPrn).ToListAsync(cancellationToken);
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
                        AimSeqNumber = (int)aecApprenticeshipPriceEpisode.PriceEpisodeAimSeqNumber,
                        AttributeName = aecApprenticeshipPriceEpisodePeriodisedValue.AttributeName,
                        Periods = new[]
                        {
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_1.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_2.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_3.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_4.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_5.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_6.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_7.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_8.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_9.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_10.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_11.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_12.GetValueOrDefault(),
                        }
                    };
                    appsAdditionalPaymentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues.Add(periodisedValue);
                }
            }

            foreach (var aecLearningDelivery in aecLearningDeliveries)
            {
                var aecLearningDeliveryInfo = new AECLearningDeliveryInfo()
                {
                    UKPRN = ukPrn,
                    LearnRefNumber = aecLearningDelivery.LearnRefNumber,
                    AimSeqNumber = aecLearningDelivery.AimSeqNumber,
                    LearnDelEmpIdFirstAdditionalPaymentThreshold = aecLearningDelivery.LearnDelEmpIdFirstAdditionalPaymentThreshold,
                    LearnDelEmpIdSecondAdditionalPaymentThreshold = aecLearningDelivery.LearnDelEmpIdSecondAdditionalPaymentThreshold
                };

                appsAdditionalPaymentRulebaseInfo.AECLearningDeliveries.Add(aecLearningDeliveryInfo);
            }

            return appsAdditionalPaymentRulebaseInfo;
        }

        public async Task<AppsMonthlyPaymentRulebaseInfo> GetRulebaseDataForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            AppsMonthlyPaymentRulebaseInfo appsMonthlyPaymentRulebaseInfo = null;
            try
            {
                appsMonthlyPaymentRulebaseInfo = new AppsMonthlyPaymentRulebaseInfo()
                {
                    UkPrn = ukPrn,
                    AecApprenticeshipPriceEpisodeInfoList = new List<AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo>(),
                    AecLearningDeliveryInfoList = new List<AppsMonthlyPaymentAECLearningDeliveryInfo>()
                };

                cancellationToken.ThrowIfCancellationRequested();

                using (var ilrContext = _ilrRulebaseContextFactory())
                {
                    appsMonthlyPaymentRulebaseInfo.AecApprenticeshipPriceEpisodeInfoList = await ilrContext?.AEC_ApprenticeshipPriceEpisodes
                        .Where(x => x.UKPRN == ukPrn)
                        .Select(ape => new AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo
                        {
                            Ukprn = (ape != null && ape.UKPRN != null) ? ape.UKPRN.ToString() : string.Empty,
                            LearnRefNumber = (ape != null && ape.LearnRefNumber != null) ? ape.LearnRefNumber : string.Empty,
                            PriceEpisodeIdentifier = (ape != null && ape.PriceEpisodeIdentifier != null) ? ape.PriceEpisodeIdentifier : string.Empty,
                            AimSequenceNumber = ape.PriceEpisodeAimSeqNumber.ToString() ?? string.Empty,
                            EpisodeStartDate = ape.EpisodeStartDate,
                            PriceEpisodeActualEndDate = ape.PriceEpisodeActualEndDate,
                            PriceEpisodeActualEndDateIncEPA = ape.PriceEpisodeActualEndDateIncEPA,
                            PriceEpisodeAgreeId = (ape != null && ape.PriceEpisodeAgreeId != null) ? ape.PriceEpisodeAgreeId : string.Empty
                        }).ToListAsync(cancellationToken);

                    appsMonthlyPaymentRulebaseInfo.AecLearningDeliveryInfoList = await ilrContext?.AEC_LearningDeliveries
                        .Where(x => x.UKPRN == ukPrn)
                        .Select(ald => new AppsMonthlyPaymentAECLearningDeliveryInfo
                        {
                            Ukprn = (ald != null && ald.UKPRN != null) ? ald.UKPRN.ToString() : string.Empty,
                            LearnRefNumber = (ald != null && ald.LearnRefNumber != null) ? ald.LearnRefNumber : string.Empty,
                            PlannedNumOnProgInstalm = (ald != null && ald.PlannedNumOnProgInstalm != null) ? ald.PlannedNumOnProgInstalm.ToString() : string.Empty,
                        }).ToListAsync(cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get Rulebase data", e);
            }

            return appsMonthlyPaymentRulebaseInfo;
        }
    }
}