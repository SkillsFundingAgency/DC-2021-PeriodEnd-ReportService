using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments
{
    public class EarningsAndPaymentsBuilder : IEarningsAndPaymentsBuilder
    {
        private static readonly string[] Type4_6 = new string[] { AttributeConstants.Fm36PriceEpisodeFirstEmp1618PayAttributeName, AttributeConstants.Fm36PriceEpisodeSecondEmp1618PayAttributeName };
        private static readonly string[] Type5_7 = new string[] { AttributeConstants.Fm36PriceEpisodeFirstProv1618PayAttributeName, AttributeConstants.Fm36PriceEpisodeSecondProv1618PayAttributeName };
        private static readonly string[] Type16 = new string[] { AttributeConstants.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName };
        private static readonly Dictionary<byte, string[]> AttributesForType = new Dictionary<byte, string[]>
        {
            [4] = Type4_6,
            [5] = Type5_7,
            [6] = Type4_6,
            [7] = Type5_7,
            [16] = Type16
        };

        public EarningsAndPayments Build(
            IEnumerable<PaymentAndLearningDelivery> paymentAndLearningDeliveries, 
            IEnumerable<ApprenticeshipPriceEpisodePeriodisedValues> periodisedValuesForLearner)
        {
            var result = new EarningsAndPayments();

            // All payments records for this reporting line get added together by period.
            var paymentsByPeriodLookup = paymentAndLearningDeliveries.GroupBy(p => p.Payment.CollectionPeriod).ToDictionary(pbp => pbp.Key);

            result.AugustR01Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)1)?.Sum(p => p.Payment.Amount) ?? 0;
            result.SeptemberR02Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)2)?.Sum(p => p.Payment.Amount) ?? 0;
            result.OctoberR03Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)3)?.Sum(p => p.Payment.Amount) ?? 0;
            result.NovemberR04Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)4)?.Sum(p => p.Payment.Amount) ?? 0;
            result.DecemberR05Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)5)?.Sum(p => p.Payment.Amount) ?? 0;
            result.JanuaryR06Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)6)?.Sum(p => p.Payment.Amount) ?? 0;
            result.FebruaryR07Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)7)?.Sum(p => p.Payment.Amount) ?? 0;
            result.MarchR08Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)8)?.Sum(p => p.Payment.Amount) ?? 0;
            result.AprilR09Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)9)?.Sum(p => p.Payment.Amount) ?? 0;
            result.MayR10Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)10)?.Sum(p => p.Payment.Amount) ?? 0;
            result.JuneR11Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)11)?.Sum(p => p.Payment.Amount) ?? 0;
            result.JulyR12Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)12)?.Sum(p => p.Payment.Amount) ?? 0;
            result.R13Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)13)?.Sum(p => p.Payment.Amount) ?? 0;
            result.R14Payments = paymentsByPeriodLookup.GetValueOrDefault((byte)14)?.Sum(p => p.Payment.Amount) ?? 0;

            result.TotalPaymentsYearToDate =
                result.AugustR01Payments + result.SeptemberR02Payments + result.OctoberR03Payments +
                result.NovemberR04Payments + result.DecemberR05Payments + result.JanuaryR06Payments +
                result.FebruaryR07Payments + result.MarchR08Payments + result.AprilR09Payments +
                result.MayR10Payments + result.JuneR11Payments + result.JulyR12Payments +
                result.R13Payments + result.R14Payments;

            //Need to compress the payment records to remove the duplicates when not looking at the collection period the payment belongs to.
            List<(int aimSeq, byte transType)> TransactionTypeAndAimSeqs = paymentAndLearningDeliveries.Select(pld =>
                    (pld.LearningDelivery?.AimSequenceNumber ?? 0, pld.Payment.TransactionType))
                .Distinct().ToList();

            // For Earnings we need to get the relevant periodised values for the aim sequence from each transaction type, then get the relevant Attribute matches
            foreach (var TransactionTypeAndAimSeq in TransactionTypeAndAimSeqs)
            {
                var periodisedValuesForPayment =
                    periodisedValuesForLearner?.Where(pv => pv.AimSeqNumber == TransactionTypeAndAimSeq.aimSeq).ToList() ??
                    new List<ApprenticeshipPriceEpisodePeriodisedValues>();

                var attributeTypesToMatch = GetAttributesForTransactionType(TransactionTypeAndAimSeq.transType);

                result.AugustEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_1);
                result.SeptemberEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_2);
                result.OctoberEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_3);
                result.NovemberEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_4);
                result.DecemberEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_5);
                result.JanuaryEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_6);
                result.FebruaryEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_7);
                result.MarchEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_8);
                result.AprilEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_9);
                result.MayEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_10);
                result.JuneEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_11);
                result.JulyEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, pvp => pvp.Period_12);
            }

            result.TotalEarnings =
                result.AugustEarnings + result.SeptemberEarnings + result.OctoberEarnings +
                result.NovemberEarnings + result.DecemberEarnings + result.JanuaryEarnings +
                result.FebruaryEarnings + result.MarchEarnings + result.AprilEarnings +
                result.MayEarnings + result.JuneEarnings + result.JulyEarnings;

            return result;
        }

        public decimal GetEarningsForPeriod(
            List<ApprenticeshipPriceEpisodePeriodisedValues> periodisedValuesForPayment, 
            string[] attributeTypes, 
            Func<ApprenticeshipPriceEpisodePeriodisedValues, decimal?> periodSelector)
        {
            var matchingAttribute = periodisedValuesForPayment
                .Where(pvp => attributeTypes.Contains(pvp.AttributeName, StringComparer.CurrentCultureIgnoreCase));

            return matchingAttribute.Select(periodSelector).Where(v => v.HasValue).Sum(p => p.Value);
        }

        public string[] GetAttributesForTransactionType(byte transactionType)
        {
            if (AttributesForType.TryGetValue(transactionType, out string[] matches))
            {
                return matches;
            }
            else
            {
                throw new ApplicationException($"Unexpected TransactionType [{transactionType}]");
            }
        }
    }
}