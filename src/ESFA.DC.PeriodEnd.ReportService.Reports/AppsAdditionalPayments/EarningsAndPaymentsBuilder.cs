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

                result.AugustEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 1);
                result.SeptemberEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 2);
                result.OctoberEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 3);
                result.NovemberEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 4);
                result.DecemberEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 5);
                result.JanuaryEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 6);
                result.FebruaryEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 7);
                result.MarchEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 8);
                result.AprilEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 9);
                result.MayEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 10);
                result.JuneEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 11);
                result.JulyEarnings += GetEarningsForPeriod(periodisedValuesForPayment, attributeTypesToMatch, 12);
            }

            result.TotalEarnings =
                result.AugustEarnings + result.SeptemberEarnings + result.OctoberEarnings +
                result.NovemberEarnings + result.DecemberEarnings + result.JanuaryEarnings +
                result.FebruaryEarnings + result.MarchEarnings + result.AprilEarnings +
                result.MayEarnings + result.JuneEarnings + result.JulyEarnings;

            return result;
        }

        public decimal GetEarningsForPeriod(List<ApprenticeshipPriceEpisodePeriodisedValues> periodisedValuesForPayment, string[] attributeTypes, int period)
        {
            var matchingAttribute = periodisedValuesForPayment
                .Where(pvp => attributeTypes.Contains(pvp.AttributeName, StringComparer.CurrentCultureIgnoreCase));

            switch (period)
            {
                case 1:
                    return matchingAttribute.Sum(pvp => pvp.Period_1) ?? 0;
                case 2:
                    return matchingAttribute.Sum(pvp => pvp.Period_2) ?? 0;
                case 3:
                    return matchingAttribute.Sum(pvp => pvp.Period_3) ?? 0;
                case 4:
                    return matchingAttribute.Sum(pvp => pvp.Period_4) ?? 0;
                case 5:
                    return matchingAttribute.Sum(pvp => pvp.Period_5) ?? 0;
                case 6:
                    return matchingAttribute.Sum(pvp => pvp.Period_6) ?? 0;
                case 7:
                    return matchingAttribute.Sum(pvp => pvp.Period_7) ?? 0;
                case 8:
                    return matchingAttribute.Sum(pvp => pvp.Period_8) ?? 0;
                case 9:
                    return matchingAttribute.Sum(pvp => pvp.Period_9) ?? 0;
                case 10:
                    return matchingAttribute.Sum(pvp => pvp.Period_10) ?? 0;
                case 11:
                    return matchingAttribute.Sum(pvp => pvp.Period_11) ?? 0;
                case 12:
                    return matchingAttribute.Sum(pvp => pvp.Period_12) ?? 0;
                default:
                    throw new ApplicationException($"Unexpected Period [{period}]");
            }
        }

        public string[] GetAttributesForTransactionType(byte transactionType)
        {
            string[] matches;
            switch (transactionType)
            {
                case DASPayments.TransactionType.First_16To18_Employer_Incentive:
                case DASPayments.TransactionType.Second_16To18_Employer_Incentive:
                    matches = Type4_6;
                    break;
                case DASPayments.TransactionType.First_16To18_Provider_Incentive:
                case DASPayments.TransactionType.Second_16To18_Provider_Incentive:
                    matches = Type5_7;
                    break;
                case DASPayments.TransactionType.Apprenticeship:
                    matches = Type16;
                    break;
                default:
                    throw new ApplicationException(
                        $"Unexpected TransactionType [{transactionType}]");
            }

            return matches;
        }
    }
}