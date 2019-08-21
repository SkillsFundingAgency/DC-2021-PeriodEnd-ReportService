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

        public async Task<AppsMonthlyPaymentRulebaseInfo> GetFM36DataForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsMonthlyPaymentRulebaseInfo = new AppsMonthlyPaymentRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECApprenticeshipPriceEpisodes = new List<AECApprenticeshipPriceEpisodeInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                var aecApprenticeshipPriceEpisodeInfos = await ilrContext.AEC_ApprenticeshipPriceEpisodes
                    .Where(x => x.UKPRN == ukPrn)
                    .Select(pe => new AECApprenticeshipPriceEpisodeInfo
                    {
                        UkPrn = pe.UKPRN,
                        AimSequenceNumber = (int)pe.PriceEpisodeAimSeqNumber,
                        LearnRefNumber = pe.LearnRefNumber,
                        PriceEpisodeActualEndDate = pe.PriceEpisodeActualEndDate,
                        PriceEpisodeAgreeId = pe.PriceEpisodeAgreeId
                    }).ToListAsync(cancellationToken);

                appsMonthlyPaymentRulebaseInfo.AECApprenticeshipPriceEpisodes.AddRange(aecApprenticeshipPriceEpisodeInfos);
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

                    appsCoInvestmentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues.Add(periodisedValue);
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
                    LearnDelEmpIdSecondAdditionalPaymentThreshold = aecLearningDelivery.LearnDelEmpIdSecondAdditionalPaymentThreshold,
                    AppAdjLearnStartDate = aecLearningDelivery.AppAdjLearnStartDate
                };

                appsCoInvestmentRulebaseInfo.AECLearningDeliveries.Add(aecLearningDeliveryInfo);
            }

            return appsCoInvestmentRulebaseInfo;
        }
    }
}