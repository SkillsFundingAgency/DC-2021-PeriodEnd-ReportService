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
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class IlrRulebaseProviderService : AbstractFundModelProviderService // ,Interface.Provider.IFm35PeriodEndProviderService
    {
        private readonly Func<IIlr1920RulebaseContext> _ilrRulebaseContextFactory;

        public IlrRulebaseProviderService(
            ILogger logger,
            Func<IIlr1920RulebaseContext> ilrRulebaseContextFactory)
            : base(logger)
        {
            _ilrRulebaseContextFactory = ilrRulebaseContextFactory;
        }

        public Dictionary<string, Dictionary<string, decimal?[][]>> GetFM35LearningDeliveryPerioisedValues(int ukprn)
        {
            var appsAdditionalPaymentRulebaseInfo = new AppsAdditionalPaymentRulebaseInfo()
            {
                UkPrn = ukprn,
                AECApprenticeshipPriceEpisodePeriodisedValues =
                    new List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>(),
                AECLearningDeliveries = new List<AECLearningDeliveryInfo>()
            };

            //CREATE TABLE[Rulebase].[FM35_LearningDelivery_PeriodisedValues]
            //    (
            //    [UKPRN][int] NOT NULL,
            //    [LearnRefNumber] [varchar] (12) NOT NULL,
            //    [AimSeqNumber] [int] NOT NULL,
            //    [AttributeName] [varchar] (100) NOT NULL,
            //    [Period_1] [decimal](15, 5) NULL,
            //    [Period_2] [decimal](15, 5) NULL,
            //    [Period_3] [decimal](15, 5) NULL,
            //    [Period_4] [decimal](15, 5) NULL,
            //    [Period_5] [decimal](15, 5) NULL,
            //    [Period_6] [decimal](15, 5) NULL,
            //    [Period_7] [decimal](15, 5) NULL,
            //    [Period_8] [decimal](15, 5) NULL,
            //    [Period_9] [decimal](15, 5) NULL,
            //    [Period_10] [decimal](15, 5) NULL,
            //    [Period_11] [decimal](15, 5) NULL,
            //    [Period_12] [decimal](15, 5) NULL,

            //
            // decimal? AchievePayment[P1][P2][P3][P4][P5][P6][P7][P8][P9][P10][P11][P12]
            // decimal? BalancePayment[P1][P2][P3][P4][P5][P6][P7][P8][P9][P10][P11][P12]

            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                var fm35LearningDeliveryPeriodisedValues = ilrContext.FM35_LearningDeliveries
                               .Where(ld => ld.UKPRN == ukprn)
                               .GroupBy(ld => ld.FundLine, StringComparer.OrdinalIgnoreCase)
                               .ToDictionary(k => k.Key,
                                   v => v.SelectMany(ld => ld.FM35_LearningDelivery_PeriodisedValues)
                                       .GroupBy(ldpv => ldpv.AttributeName, StringComparer.OrdinalIgnoreCase)
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
    }
}