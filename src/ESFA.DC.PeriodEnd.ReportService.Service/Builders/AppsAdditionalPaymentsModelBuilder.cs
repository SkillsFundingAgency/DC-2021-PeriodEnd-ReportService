using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class AppsAdditionalPaymentsModelBuilder : IAppsAdditionalPaymentsModelBuilder
    {
        public IEnumerable<AppsAdditionalPaymentsModel> BuildModel(
            AppsAdditionalPaymentILRInfo appsAdditionalPaymentIlrInfo,
            AppsAdditionalPaymentRulebaseInfo appsAdditionalPaymentRulebaseInfo,
            AppsAdditionalPaymentDasPaymentsInfo appsAdditionalPaymentDasPaymentsInfo)
        {
            List<AppsAdditionalPaymentsModel> appsAdditionalPaymentsModels = new List<AppsAdditionalPaymentsModel>();

            // Create an extended payments model that includes the payment related ILR data
            var extendedPayments = BuildAdditionalPaymentsExtendedPaymentsModel(
                appsAdditionalPaymentIlrInfo,
                appsAdditionalPaymentRulebaseInfo,
                appsAdditionalPaymentDasPaymentsInfo);

            /*
             * Group the rows by BR1 for the final report
             *    LearnerReferenceNumber
             *    UniqueLearnerNumber
             *    LearningStartDate
             *    FundingLineType
             *    TypeOfAdditionalPayment
             *    AppServiceEmployerName
             *    IlrEmployerIdentifier
             */
            var additionalPaymentGroups = extendedPayments
                .GroupBy(x => new
                {
                    x.PaymentLearnerReferenceNumber,
                    x.PaymentUniqueLearnerNumber,
                    x.PaymentLearningStartDate,
                    x.PaymentLearningAimFundingLineType,
                    x.PaymentTypeOfAdditionalPayment,
                    x.AppsServiceEmployerName,
                    x.ilrEmployerIdentifier
                })
                .OrderBy(o => o.Key.PaymentLearnerReferenceNumber)
                .ThenBy(o => o.Key.PaymentUniqueLearnerNumber)
                .ThenBy(o => o.Key.PaymentLearningStartDate)
                .ThenBy(o => o.Key.PaymentLearningAimFundingLineType)
                .ThenBy(o => o.Key.PaymentTypeOfAdditionalPayment)
                .ThenBy(o => o.Key.AppsServiceEmployerName)
                .ThenBy(o => o.Key.ilrEmployerIdentifier)
                .Select(g => new AppsAdditionalPaymentsModel
                {
                    // group key fields
                    LearnerReferenceNumber = g.Key.PaymentLearnerReferenceNumber,
                    UniqueLearnerNumber = g.Key.PaymentUniqueLearnerNumber,
                    LearningStartDate = g.Key.PaymentLearningStartDate,
                    FundingLineType = g.Key.PaymentLearningAimFundingLineType,
                    TypeOfAdditionalPayment = g.Key.PaymentTypeOfAdditionalPayment,
                    EmployerNameFromApprenticeshipService = g.Key.AppsServiceEmployerName,
                    EmployerIdentifierFromILR = g.Key.ilrEmployerIdentifier,

                    // other fields
                    ProviderSpecifiedLearnerMonitoringA = g?.FirstOrDefault()?.ProviderSpecifiedLearnerMonitoringA,
                    ProviderSpecifiedLearnerMonitoringB = g?.FirstOrDefault()?.ProviderSpecifiedLearnerMonitoringB,

                    // period totals
                    AugustEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    AugustR01Payments = g.Where(p => PeriodPaymentsPredicate(p, 1)).Sum(c => c.PaymentAmount ?? 0m),
                    SeptemberEarnings = g.Where(p => PeriodEarningsPredicate(p, 2)).Sum(c => c.EarningAmount ?? 0m),
                    SeptemberR02Payments = g.Where(p => PeriodPaymentsPredicate(p, 2)).Sum(c => c.PaymentAmount ?? 0m),
                    OctoberEarnings = g.Where(p => PeriodEarningsPredicate(p, 3)).Sum(c => c.EarningAmount ?? 0m),
                    OctoberR03Payments = g.Where(p => PeriodPaymentsPredicate(p, 3)).Sum(c => c.PaymentAmount ?? 0m),
                    NovemberEarnings = g.Where(p => PeriodEarningsPredicate(p, 4)).Sum(c => c.EarningAmount ?? 0m),
                    NovemberR04Payments = g.Where(p => PeriodPaymentsPredicate(p, 4)).Sum(c => c.PaymentAmount ?? 0m),
                    DecemberEarnings = g.Where(p => PeriodEarningsPredicate(p, 5)).Sum(c => c.EarningAmount ?? 0m),
                    DecemberR05Payments = g.Where(p => PeriodPaymentsPredicate(p, 5)).Sum(c => c.PaymentAmount ?? 0m),
                    JanuaryEarnings = g.Where(p => PeriodEarningsPredicate(p, 6)).Sum(c => c.EarningAmount ?? 0m),
                    JanuaryR06Payments = g.Where(p => PeriodPaymentsPredicate(p, 6)).Sum(c => c.PaymentAmount ?? 0m),
                    FebruaryEarnings = g.Where(p => PeriodEarningsPredicate(p, 7)).Sum(c => c.EarningAmount ?? 0m),
                    FebruaryR07Payments = g.Where(p => PeriodPaymentsPredicate(p, 7)).Sum(c => c.PaymentAmount ?? 0m),
                    MarchEarnings = g.Where(p => PeriodEarningsPredicate(p, 8)).Sum(c => c.EarningAmount ?? 0m),
                    MarchR08Payments = g.Where(p => PeriodPaymentsPredicate(p, 8)).Sum(c => c.PaymentAmount ?? 0m),
                    AprilEarnings = g.Where(p => PeriodEarningsPredicate(p, 9)).Sum(c => c.EarningAmount ?? 0m),
                    AprilR09Payments = g.Where(p => PeriodPaymentsPredicate(p, 9)).Sum(c => c.PaymentAmount ?? 0m),
                    MayEarnings = g.Where(p => PeriodEarningsPredicate(p, 10)).Sum(c => c.EarningAmount ?? 0m),
                    MayR10Payments = g.Where(p => PeriodPaymentsPredicate(p, 10)).Sum(c => c.PaymentAmount ?? 0m),
                    JuneEarnings = g.Where(p => PeriodEarningsPredicate(p, 11)).Sum(c => c.EarningAmount ?? 0m),
                    JuneR11Payments = g.Where(p => PeriodPaymentsPredicate(p, 11)).Sum(c => c.PaymentAmount ?? 0m),
                    JulyEarnings = g.Where(p => PeriodEarningsPredicate(p, 12)).Sum(c => c.EarningAmount ?? 0m),
                    JulyR12Payments = g.Where(p => PeriodPaymentsPredicate(p, 12)).Sum(c => c.PaymentAmount ?? 0m),
                    R13Payments = g.Where(p => PeriodPaymentsPredicate(p, 13)).Sum(c => c.EarningAmount ?? 0m),
                    R14Payments = g.Where(p => PeriodPaymentsPredicate(p, 14)).Sum(c => c.PaymentAmount ?? 0m),

                    // Annual totals
                    TotalEarnings = g.Where(p => AnnualEarningsPredicate(p, 0)).Sum(c => c.EarningAmount ?? 0m),
                    TotalPaymentsYearToDate = g.Where(p => AnnualPaymentsPredicate(p, 0)).Sum(c => c.PaymentAmount ?? 0m),
                }).ToList();

            appsAdditionalPaymentsModels.AddRange(additionalPaymentGroups);

            return appsAdditionalPaymentsModels;
        }

        /// <summary>
        /// Create an extended payments model that includes the payment related ILR data
        /// </summary>
        /// <param name="appsAdditionalPaymentIlrInfo"></param>
        /// <param name="appsAdditionalPaymentRulebaseInfo"></param>
        /// <param name="appsAdditionalPaymentDasPaymentsInfo"></param>
        /// <returns>A list of AppsAdditionalPaymentExtendedPaymentModels.</returns>
        public List<AppsAdditionalPaymentExtendedPaymentModel> BuildAdditionalPaymentsExtendedPaymentsModel(
            AppsAdditionalPaymentILRInfo appsAdditionalPaymentIlrInfo,
            AppsAdditionalPaymentRulebaseInfo appsAdditionalPaymentRulebaseInfo,
            AppsAdditionalPaymentDasPaymentsInfo appsAdditionalPaymentDasPaymentsInfo)
        {
            List<AppsAdditionalPaymentExtendedPaymentModel> extendedPayments =
                new List<AppsAdditionalPaymentExtendedPaymentModel>();

            // Create a new payment model which includes the related data from the ILR
            foreach (var dasPaymentInfo in appsAdditionalPaymentDasPaymentsInfo.Payments)
            {
                // lookup the related reference data for this payment
                var learner = appsAdditionalPaymentIlrInfo?.Learners?
                    .SingleOrDefault(x =>
                        x.LearnRefNumber.CaseInsensitiveEquals(dasPaymentInfo?.LearnerReferenceNumber));

                var learningDelivery = learner?.LearningDeliveries?.SingleOrDefault(x =>
                        (x?.LearnRefNumber.CaseInsensitiveEquals(dasPaymentInfo?.LearnerReferenceNumber) ?? false) &&
                        x.LearnAimRef.CaseInsensitiveEquals(dasPaymentInfo?.LearningAimReference) &&
                        x?.LearnStartDate == dasPaymentInfo?.LearningStartDate &&
                        (x?.ProgType == null || x?.ProgType == dasPaymentInfo?.LearningAimProgrammeType) &&
                        (x?.StdCode == null || x?.StdCode == dasPaymentInfo?.LearningAimStandardCode) &&
                        (x?.FworkCode == null || x?.FworkCode == dasPaymentInfo?.LearningAimFrameworkCode) &&
                        (x?.PwayCode == null || x?.PwayCode == dasPaymentInfo?.LearningAimPathwayCode));

                var aecLearningDelivery = appsAdditionalPaymentRulebaseInfo?.AECLearningDeliveries?.SingleOrDefault(x =>
                        x?.UKPRN == learningDelivery?.UKPRN &&
                        (x?.LearnRefNumber.CaseInsensitiveEquals(learningDelivery?.LearnRefNumber) ?? false) &&
                        x?.AimSeqNumber == learningDelivery?.AimSeqNumber);

                var aecApprenticeshipPriceEpisodePeriodisedValues = appsAdditionalPaymentRulebaseInfo
                    ?.AECApprenticeshipPriceEpisodePeriodisedValues.Where(x =>
                        x?.UKPRN == learningDelivery?.UKPRN &&
                        (x?.LearnRefNumber.CaseInsensitiveEquals(learningDelivery?.LearnRefNumber) ?? false) &&
                        x?.AimSeqNumber == learningDelivery?.AimSeqNumber).ToList();

                // copy this payment's fields to the new extended payment model
                var extendedPayment = new AppsAdditionalPaymentExtendedPaymentModel();

                // copy the reporting grouping fields
                extendedPayment.PaymentLearnerReferenceNumber = dasPaymentInfo.LearnerReferenceNumber;
                extendedPayment.PaymentUniqueLearnerNumber = dasPaymentInfo.LearnerUln;
                extendedPayment.PaymentLearningStartDate = dasPaymentInfo.LearningStartDate;
                extendedPayment.PaymentLearningAimFundingLineType = dasPaymentInfo.LearningAimFundingLineType;
                extendedPayment.PaymentTypeOfAdditionalPayment = dasPaymentInfo.TypeOfAdditionalPayment;
                extendedPayment.AppsServiceEmployerName = dasPaymentInfo.EmployerName;
                extendedPayment.ilrEmployerIdentifier = GetEmployerIdentifier(aecLearningDelivery, dasPaymentInfo);

                // copy the remaining payment fields
                extendedPayment.PaymentLearningAimProgrammeType = dasPaymentInfo.LearningAimProgrammeType;
                extendedPayment.PaymentLearningAimStandardCode = dasPaymentInfo.LearningAimStandardCode;
                extendedPayment.PaymentLearningAimFrameworkCode = dasPaymentInfo.LearningAimFrameworkCode;
                extendedPayment.PaymentLearningAimPathwayCode = dasPaymentInfo.LearningAimPathwayCode;
                extendedPayment.PaymentLearningAimReference = dasPaymentInfo.LearningAimReference;
                extendedPayment.PaymentContractType = dasPaymentInfo.ContractType;
                extendedPayment.PaymentFundingSource = dasPaymentInfo.FundingSource;
                extendedPayment.PaymentTransactionType = dasPaymentInfo.TransactionType;
                extendedPayment.PaymentAcademicYear = dasPaymentInfo.AcademicYear;
                extendedPayment.PaymentCollectionPeriod = dasPaymentInfo.CollectionPeriod;
                extendedPayment.PaymentDeliveryPeriod = dasPaymentInfo.DeliveryPeriod;
                extendedPayment.PaymentAmount = dasPaymentInfo.Amount;

                // copy the ilr fields
                extendedPayment.ProviderSpecifiedLearnerMonitoringA = GetProviderSpecMonitor(learner, Generics.ProviderSpecifiedLearnerMonitoringA);
                extendedPayment.ProviderSpecifiedLearnerMonitoringB = GetProviderSpecMonitor(learner, Generics.ProviderSpecifiedLearnerMonitoringB);
                extendedPayment.EarningAmount = GetMonthlyEarnings(dasPaymentInfo, aecApprenticeshipPriceEpisodePeriodisedValues, dasPaymentInfo.CollectionPeriod);

                extendedPayments.Add(extendedPayment);
            }

            return extendedPayments;
        }

        private string GetProviderSpecMonitor(AppsAdditionalPaymentLearnerInfo learner, string providerSpecifiedLearnerMonitoring)
        {
            return learner?.ProviderSpecLearnerMonitorings
                       .SingleOrDefault(x => x.ProvSpecLearnMonOccur.CaseInsensitiveEquals(providerSpecifiedLearnerMonitoring))?.ProvSpecLearnMon ?? string.Empty;
        }

        private bool PeriodPaymentsPredicate(AppsAdditionalPaymentExtendedPaymentModel payment, int period)
        {
            bool result = payment.PaymentCollectionPeriod == period &&
                          AnnualPaymentsPredicate(payment, 0);

            return result;
        }

        private bool AnnualPaymentsPredicate(AppsAdditionalPaymentExtendedPaymentModel payment, int period)
        {
            bool result = payment.PaymentAcademicYear == 1920 &&
                          (payment.PaymentTypeOfAdditionalPayment.CaseInsensitiveEquals("Employer") ||
                           payment.PaymentTypeOfAdditionalPayment.CaseInsensitiveEquals("Provider") ||
                           payment.PaymentTypeOfAdditionalPayment.CaseInsensitiveEquals("Apprentice"));

            return result;
        }

        private bool PeriodEarningsPredicate(AppsAdditionalPaymentExtendedPaymentModel payment, int period)
        {
            bool result = payment.PaymentCollectionPeriod == period &&
                          AnnualEarningsPredicate(payment, period);

            return result;
        }

        private bool AnnualEarningsPredicate(AppsAdditionalPaymentExtendedPaymentModel payment, int period)
        {
            bool result = payment.PaymentAcademicYear == 1920;

            return result;
        }

        private decimal GetMonthlyEarnings(
            DASPaymentInfo dasPaymentInfo,
            List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> aecApprenticeshipPriceEpisodePeriodisedValues,
            byte period)
        {
            decimal earnings = 0m;

            if (dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive || dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive)
            {
                earnings = GetAECPriceEpisodePeriodisedValue(dasPaymentInfo.LearnerReferenceNumber, aecApprenticeshipPriceEpisodePeriodisedValues, Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName, period)
                             + GetAECPriceEpisodePeriodisedValue(dasPaymentInfo.LearnerReferenceNumber, aecApprenticeshipPriceEpisodePeriodisedValues, Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName, period);
            }
            else if (dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive || dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive)
            {
                earnings = GetAECPriceEpisodePeriodisedValue(dasPaymentInfo.LearnerReferenceNumber, aecApprenticeshipPriceEpisodePeriodisedValues, Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName, period)
                             + GetAECPriceEpisodePeriodisedValue(dasPaymentInfo.LearnerReferenceNumber, aecApprenticeshipPriceEpisodePeriodisedValues, Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName, period);
            }
            else if (dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.Apprenticeship)
            {
                earnings = GetAECPriceEpisodePeriodisedValue(dasPaymentInfo.LearnerReferenceNumber, aecApprenticeshipPriceEpisodePeriodisedValues, Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName, period);
            }

            return earnings;
        }

        private decimal GetAECPriceEpisodePeriodisedValue(
            string learnRefNumber,
            List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> aecApprenticeshipPriceEpisodePeriodisedValues,
            string attributeName,
            byte period)
        {
            var amount = aecApprenticeshipPriceEpisodePeriodisedValues
                             .Where(x =>
                                 (x?.LearnRefNumber.CaseInsensitiveEquals(learnRefNumber) ?? false) &&
                                 (x?.AttributeName.CaseInsensitiveEquals(attributeName) ?? false) &&
                                 GetDateFromPriceEpisodeIdentifier(x?.PriceEpisodeIdentifier) >= Generics.BeginningOfYear)
                             .Sum(z => z?.Periods[period - 1]) ?? 0;

            return amount;
        }

        private DateTime GetDateFromPriceEpisodeIdentifier(string priceEpisodeIdentifier)
        {
            string dateElement = string.Empty;
            DateTime priceEpisodeDate = DateTime.MinValue;

            if (priceEpisodeIdentifier != null && priceEpisodeIdentifier.Length >= 10)
            {
                dateElement = priceEpisodeIdentifier.Substring(priceEpisodeIdentifier.Length - 10, 10);

                // expecting the last 10 characters of the price episode identifier to be a date in the format dd/mm/yyyy
                if (dateElement.Length == 10)
                {
                    priceEpisodeDate = DateTime.ParseExact(dateElement, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
            }

            return priceEpisodeDate;
        }

        private string GetEmployerIdentifier(AECLearningDeliveryInfo aecLearningDeliveryInfo, DASPaymentInfo payment)
        {
            var identifier = 0;

            if (aecLearningDeliveryInfo != null)
            {
                if (payment.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive) // 4
                {
                    identifier = aecLearningDeliveryInfo.LearnDelEmpIdFirstAdditionalPaymentThreshold.GetValueOrDefault();
                }
                else if (payment.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive) //6
                {
                    identifier = aecLearningDeliveryInfo.LearnDelEmpIdSecondAdditionalPaymentThreshold.GetValueOrDefault();
                }
            }

            return identifier == 0 ? Generics.NotAvailable : identifier.ToString();
        }
    }
}
