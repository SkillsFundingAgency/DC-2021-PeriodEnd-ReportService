using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR2021.DataStore.EF.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Constants;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Provider.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Provider
{
    public class FM36PeriodEndProviderService : AbstractFundModelProviderService, IFM36PeriodEndProviderService
    {
        private readonly Func<IIlr2021RulebaseContext> _ilrRulebaseContextFactory;

        public FM36PeriodEndProviderService(
            ILogger logger,
            Func<IIlr2021RulebaseContext> ilrRulebaseContextFactory)
            : base(logger)
        {
            _ilrRulebaseContextFactory = ilrRulebaseContextFactory;
        }

        public async Task<List<AECLearningDeliveryInfo>> GetLearningDeliveriesForAppsAdditionalPaymentReportAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                return await ilrContext
                    .AEC_LearningDeliveries
                    .Where(ld => ld.UKPRN == ukprn)
                    .Select(ld =>
                        new AECLearningDeliveryInfo()
                        {
                            UKPRN = ld.UKPRN,
                            LearnRefNumber = ld.LearnRefNumber,
                            AimSeqNumber = ld.AimSeqNumber,
                            LearnDelEmpIdFirstAdditionalPaymentThreshold = ld.LearnDelEmpIdFirstAdditionalPaymentThreshold,
                            LearnDelEmpIdSecondAdditionalPaymentThreshold = ld.LearnDelEmpIdSecondAdditionalPaymentThreshold
                        })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>> GetApprenticeshipPriceEpisodesForAppsAdditionalPaymentsReportAsync(int ukprn, CancellationToken cancellationToken)
        {
            IEnumerable<string> applicableAttributeNames = new[]
            {
                Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName,
                Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName,
                Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName,
                Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName,
                Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName
            };

            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                return await ilrContext
                    .AEC_ApprenticeshipPriceEpisode_PeriodisedValues
                    .Where(x => x.UKPRN == ukprn && applicableAttributeNames.Contains(x.AttributeName))
                    .Select(pv => new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                    {
                        UKPRN = pv.UKPRN,
                        LearnRefNumber = pv.LearnRefNumber,
                        AimSeqNumber = pv.AEC_ApprenticeshipPriceEpisode.PriceEpisodeAimSeqNumber,
                        AttributeName = pv.AttributeName,
                        PriceEpisodeIdentifier = pv.PriceEpisodeIdentifier,
                        Periods = new[]
                                {
                                    pv.Period_1,
                                    pv.Period_2,
                                    pv.Period_3,
                                    pv.Period_4,
                                    pv.Period_5,
                                    pv.Period_6,
                                    pv.Period_7,
                                    pv.Period_8,
                                    pv.Period_9,
                                    pv.Period_10,
                                    pv.Period_11,
                                    pv.Period_12,
                                }
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<AppsMonthlyPaymentRulebaseInfo> GetRulebaseDataForAppsMonthlyPaymentReportAsync(
            int ukPrn,
            CancellationToken cancellationToken)
        {
            AppsMonthlyPaymentRulebaseInfo appsMonthlyPaymentRulebaseInfo = null;

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

        public async Task<LearnerLevelViewFM36Info> GetFM36DataForLearnerLevelView(int ukPrn, CancellationToken cancellationToken)
        {
            var learnerLevelInfo = new LearnerLevelViewFM36Info()
            {
                UkPrn = ukPrn
            };

            // Looking for period data on PriceEpisodes and Learningeliveries
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var ilrContext = _ilrRulebaseContextFactory())
                {
                    learnerLevelInfo.AECApprenticeshipPriceEpisodePeriodisedValues = await ilrContext
                        .AEC_ApprenticeshipPriceEpisode_PeriodisedValues
                        .Where(x => x.UKPRN == ukPrn && (x.AttributeName == Generics.Fm36PriceEpisodeCompletionPaymentAttributeName ||
                                                         x.AttributeName == Generics.Fm36PriceEpisodeOnProgPaymentAttributeName ||
                                                         x.AttributeName == Generics.Fm3PriceEpisodeBalancePaymentAttributeName ||
                                                         x.AttributeName == Generics.Fm36PriceEpisodeLSFCashAttributeName))
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

                    learnerLevelInfo.AECPriceEpisodeFLTsInfo = await ilrContext.AEC_ApprenticeshipPriceEpisodes.Where(p => p.UKPRN == ukPrn).
                        Select(pe => new LearnerLevelViewEarningsFLT()
                        {
                            LearnerReferenceNumber = pe.LearnRefNumber ?? string.Empty,
                            PaymentFundingLineType = pe.PriceEpisodeFundLineType ?? string.Empty
                        }).ToListAsync(cancellationToken);

                    learnerLevelInfo.AECLearningDeliveryPeriodisedValuesInfo = await ilrContext
                        .AEC_LearningDelivery_PeriodisedValues
                        .Where(x => x.UKPRN == ukPrn && (x.AttributeName == Generics.Fm36MathEngOnProgPaymentAttributeName ||
                                                         x.AttributeName == Generics.Fm36LearnSuppFundCashAttributeName ||
                                                         x.AttributeName == Generics.Fm36MathEngBalPayment ||
                                                         x.AttributeName == Generics.Fm36DisadvFirstPayment ||
                                                         x.AttributeName == Generics.Fm36DisadvSecondPayment ||
                                                         x.AttributeName == Generics.Fm36LearnDelFirstEmp1618Pay ||
                                                         x.AttributeName == Generics.Fm36LearnDelSecondEmp1618Pay ||
                                                         x.AttributeName == Generics.Fm36LearnDelFirstProv1618Pay ||
                                                         x.AttributeName == Generics.Fm36LearnDelSecondProv1618Pay ||
                                                         x.AttributeName == Generics.Fm36LearnDelLearnAddPayment ||
                                                         x.AttributeName == Generics.Fm36LDApplic1618FrameworkUpliftBalancingPayment ||
                                                         x.AttributeName == Generics.Fm36LDApplic1618FrameworkUpliftCompletionPayment ||
                                                         x.AttributeName == Generics.Fm36LDApplic1618FrameworkUpliftOnProgPayment))
                        .Select(ld => new AECLearningDeliveryPeriodisedValuesInfo()
                        {
                            UKPRN = ukPrn,
                            LearnRefNumber = ld.LearnRefNumber,
                            AimSeqNumber = (int?)ld.AimSeqNumber,
                            AttributeName = ld.AttributeName,
                            LearnDelMathEng = ld.AEC_LearningDelivery.LearnDelMathEng,
                            Periods = new[]
                            {
                            ld.Period_1,
                            ld.Period_2,
                            ld.Period_3,
                            ld.Period_4,
                            ld.Period_5,
                            ld.Period_6,
                            ld.Period_7,
                            ld.Period_8,
                            ld.Period_9,
                            ld.Period_10,
                            ld.Period_11,
                            ld.Period_12
                            }
                        }).ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get FM36 datam", ex);
                throw ex;
            }

            return learnerLevelInfo;
        }
    }
}