using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.AppsAdditionalPaymentsReport.Model;
using AppsAdditionalPaymentExtendedPaymentModel = ESFA.DC.PeriodEnd.ReportService.Service.Reports.AppsAdditionalPaymentsReport.Model.AppsAdditionalPaymentExtendedPaymentModel;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.AppsAdditionalPaymentsReport
{
    public class AppsAdditionalPaymentsModelBuilder : IAppsAdditionalPaymentsModelBuilder
    {
        private const string EmployerPaymentType = "Employer";
        private const string ProviderPaymentType = "Provider";
        private const string ApprenticePaymentType = "Apprentice";

        private readonly IEnumerable<string> _applicablePaymentTypes = new HashSet<string>()
        {
            EmployerPaymentType,
            ProviderPaymentType,
            ApprenticePaymentType,
        };

        private readonly IDictionary<byte, string> _transactionTypeToPaymentTypeDictionary = new Dictionary<byte, string>()
        {
            [Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive] = EmployerPaymentType,
            [Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive] = EmployerPaymentType,
            [Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive] = ProviderPaymentType,
            [Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive] = ProviderPaymentType,
            [Constants.DASPayments.TransactionType.Apprenticeship] = ApprenticePaymentType,
        };

        private readonly List<AppsAdditionalPaymentExtendedPaymentModel> _emptyModels = new List<AppsAdditionalPaymentExtendedPaymentModel>();

        public IEnumerable<AppsAdditionalPaymentsModel> BuildModel(
            IList<AppsAdditionalPaymentLearnerInfo> appsAdditionalPaymentIlrInfo,
            IList<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> rulebasePriceEpisodes,
            IList<AECLearningDeliveryInfo> rulebaseLearningDeliveries,
            IList<DASPaymentInfo> appsAdditionalPaymentDasPaymentsInfo,
            IDictionary<long, string> legalNameDictionary)
        {
            // Create an extended payments model that includes the payment related ILR data
            var extendedPayments = BuildAdditionalPaymentsExtendedPaymentsModel(
                appsAdditionalPaymentIlrInfo,
                rulebasePriceEpisodes,
                rulebaseLearningDeliveries,
                appsAdditionalPaymentDasPaymentsInfo,
                legalNameDictionary);

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
            return extendedPayments
                .GroupBy(x => new AppsAdditionalPaymentRecordKey(
                    x.PaymentLearnerReferenceNumber,
                    x.PaymentUniqueLearnerNumber,
                    x.PaymentLearningStartDate,
                    x.PaymentLearningAimFundingLineType,
                    x.PaymentTypeOfAdditionalPayment,
                    x.AppsServiceEmployerName,
                    x.ilrEmployerIdentifier
                ), AppsAdditionalPaymentRecordKey.AppsAdditionalPaymentRecordKeyComparer)
                .Select(g =>
                {
                    var academicYearPayments = g.Where(p => p.PaymentAcademicYear == Generics.AcademicYear).ToList();

                    var groupedAcademicYear = academicYearPayments
                        .GroupBy(p => p.PaymentCollectionPeriod)
                        .ToDictionary(pg => pg.Key, pg => pg.ToList());

                    var firstModel = g.FirstOrDefault();

                    return new AppsAdditionalPaymentsModel
                    {
                        // group key fields
                        LearnerReferenceNumber = g.Key.PaymentLearnerReferenceNumber,
                        UniqueLearnerNumber = g.Key.PaymentUniqueLearnerNumber,
                        LearningStartDate = g.Key.PaymentLearningStartDate,
                        FundingLineType = g.Key.PaymentLearningAimFundingLineType,
                        TypeOfAdditionalPayment = g.Key.PaymentTypeOfAdditionalPayment,
                        EmployerNameFromApprenticeshipService = g.Key.AppsServiceEmployerName,
                        EmployerIdentifierFromILR = g.Key.IlrEmployerIdentifier,

                        // other fields
                        ProviderSpecifiedLearnerMonitoringA = firstModel?.ProviderSpecifiedLearnerMonitoringA,
                        ProviderSpecifiedLearnerMonitoringB = firstModel?.ProviderSpecifiedLearnerMonitoringB,

                        // period totals
                        AugustEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 1),
                        AugustR01Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 1),
                        SeptemberEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 2),
                        SeptemberR02Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 2),
                        OctoberEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 3),
                        OctoberR03Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 3),
                        NovemberEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 4),
                        NovemberR04Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 4),
                        DecemberEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 5),
                        DecemberR05Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 5),
                        JanuaryEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 6),
                        JanuaryR06Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 6),
                        FebruaryEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 7),
                        FebruaryR07Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 7),
                        MarchEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 8),
                        MarchR08Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 8),
                        AprilEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 9),
                        AprilR09Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 9),
                        MayEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 10),
                        MayR10Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 10),
                        JuneEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 11),
                        JuneR11Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 11),
                        JulyEarnings = GetPeriodEarningsTotalForPeriod(groupedAcademicYear, 12),
                        JulyR12Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 12),
                        R13Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 13),
                        R14Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 14),

                        // Annual totals
                        TotalEarnings = academicYearPayments.Where(p => p.EarningAmount.HasValue).Sum(c => c.EarningAmount.Value),
                        TotalPaymentsYearToDate = academicYearPayments.Where(p => p.PaymentAmount.HasValue && _applicablePaymentTypes.Contains(p.PaymentTypeOfAdditionalPayment)).Sum(c => c.PaymentAmount.Value),
                    };
                })
                .OrderBy(o => o.LearnerReferenceNumber)
                .ToList();
        }

        /// <summary>
        /// Create an extended payments model that includes the payment related ILR data
        /// </summary>
        /// <param name="appsAdditionalPaymentIlrInfo"></param>
        /// <param name="appsAdditionalPaymentRulebaseInfo"></param>
        /// <param name="appsAdditionalPaymentDasPaymentsInfo"></param>
        /// <returns>A list of AppsAdditionalPaymentExtendedPaymentModels.</returns>
        public List<AppsAdditionalPaymentExtendedPaymentModel> BuildAdditionalPaymentsExtendedPaymentsModel(
            IList<AppsAdditionalPaymentLearnerInfo> appsAdditionalPaymentIlrInfo,
            IList<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> rulebasePriceEpisodes,
            IList<AECLearningDeliveryInfo> rulebaseLearningDeliveries,
            IList<DASPaymentInfo> appsAdditionalPaymentDasPaymentsInfo,
            IDictionary<long, string> legalNameDictionary)
        {
            List<AppsAdditionalPaymentExtendedPaymentModel> extendedPayments = new List<AppsAdditionalPaymentExtendedPaymentModel>();

            var learnerDictionary = appsAdditionalPaymentIlrInfo.ToDictionary(l => l.LearnRefNumber, l => l, StringComparer.OrdinalIgnoreCase);

            // Create a new payment model which includes the related data from the ILR
            foreach (var dasPaymentInfo in appsAdditionalPaymentDasPaymentsInfo.Where(p => p != null))
            {
                // lookup the related reference data for this payment
                var learner = learnerDictionary.GetValueOrDefault(dasPaymentInfo?.LearnerReferenceNumber);

                var learningDelivery = learner?.LearningDeliveries?.SingleOrDefault(x =>
                        (x?.LearnRefNumber.CaseInsensitiveEquals(dasPaymentInfo?.LearnerReferenceNumber) ?? false) &&
                        x.LearnAimRef.CaseInsensitiveEquals(dasPaymentInfo?.LearningAimReference) &&
                        x.LearnStartDate == dasPaymentInfo?.LearningStartDate &&
                        (x.ProgType == null || x.ProgType == dasPaymentInfo.LearningAimProgrammeType) &&
                        (x.StdCode == null || x.StdCode == dasPaymentInfo.LearningAimStandardCode) &&
                        (x.FworkCode == null || x.FworkCode == dasPaymentInfo.LearningAimFrameworkCode) &&
                        (x.PwayCode == null || x.PwayCode == dasPaymentInfo.LearningAimPathwayCode));

                var aecLearningDelivery = rulebaseLearningDeliveries?.FirstOrDefault(x =>
                        x?.UKPRN == learningDelivery?.UKPRN &&
                        (x?.LearnRefNumber.CaseInsensitiveEquals(learningDelivery?.LearnRefNumber) ?? false) &&
                        x.AimSeqNumber == learningDelivery.AimSeqNumber);

                var aecApprenticeshipPriceEpisodePeriodisedValues = rulebasePriceEpisodes?
                        .Where(x =>
                        x?.UKPRN == learningDelivery?.UKPRN &&
                        (x?.LearnRefNumber.CaseInsensitiveEquals(learningDelivery?.LearnRefNumber) ?? false) &&
                        x.AimSeqNumber == learningDelivery?.AimSeqNumber).ToList();

                // copy this payment's fields to the new extended payment model
                var extendedPayment = new AppsAdditionalPaymentExtendedPaymentModel();

                // copy the reporting grouping fields
                extendedPayment.PaymentLearnerReferenceNumber = dasPaymentInfo.LearnerReferenceNumber;
                extendedPayment.PaymentUniqueLearnerNumber = dasPaymentInfo.LearnerUln;
                extendedPayment.PaymentLearningStartDate = dasPaymentInfo.LearningStartDate;
                extendedPayment.PaymentLearningAimFundingLineType = dasPaymentInfo.LearningAimFundingLineType;
                extendedPayment.PaymentTypeOfAdditionalPayment = GetTypeOfAdditionalPayment(dasPaymentInfo.TransactionType);
                extendedPayment.AppsServiceEmployerName = GetAppServiceEmployerName(dasPaymentInfo, legalNameDictionary);
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

        private decimal GetPeriodEarningsTotalForPeriod(IDictionary<byte, List<AppsAdditionalPaymentExtendedPaymentModel>> models, byte period)
        {
            return models.GetValueOrDefault(period, _emptyModels).Where(p => p.EarningAmount.HasValue).Sum(c => c.EarningAmount.Value);
        }

        private decimal GetPeriodPaymentsTotalForPeriod(IDictionary<byte, List<AppsAdditionalPaymentExtendedPaymentModel>> models, byte period)
        {
            return models.GetValueOrDefault(period, _emptyModels).Where(p => p.PaymentAmount.HasValue && _applicablePaymentTypes.Contains(p.PaymentTypeOfAdditionalPayment)).Sum(c => c.PaymentAmount.Value);
        }

        private string GetTypeOfAdditionalPayment(byte transactionType) => _transactionTypeToPaymentTypeDictionary.GetValueOrDefault(transactionType, string.Empty);

        private string GetAppServiceEmployerName(DASPaymentInfo payment, IDictionary<long, string> legalNameDictionary)
        {
            if (payment.ContractType == 1 && (payment.TransactionType == 4 || payment.TransactionType == 6) && payment.ApprenticeshipId.HasValue)
            {
                return legalNameDictionary.GetValueOrDefault(payment.ApprenticeshipId.Value);
            }

            return null;
        }

        private string GetProviderSpecMonitor(AppsAdditionalPaymentLearnerInfo learner, string providerSpecifiedLearnerMonitoring)
        {
            return learner?.ProviderSpecLearnerMonitorings
                       .SingleOrDefault(x => x.ProvSpecLearnMonOccur.CaseInsensitiveEquals(providerSpecifiedLearnerMonitoring))?.ProvSpecLearnMon ?? string.Empty;
        }

        private decimal GetMonthlyEarnings(
            DASPaymentInfo dasPaymentInfo,
            List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> aecApprenticeshipPriceEpisodePeriodisedValues,
            byte period)
        {
            if (dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive || dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive)
            {
                return GetAECPriceEpisodePeriodisedValue(dasPaymentInfo.LearnerReferenceNumber, aecApprenticeshipPriceEpisodePeriodisedValues, Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName, period)
                             + GetAECPriceEpisodePeriodisedValue(dasPaymentInfo.LearnerReferenceNumber, aecApprenticeshipPriceEpisodePeriodisedValues, Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName, period);
            }

            if (dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive || dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive)
            {
                return GetAECPriceEpisodePeriodisedValue(dasPaymentInfo.LearnerReferenceNumber, aecApprenticeshipPriceEpisodePeriodisedValues, Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName, period)
                             + GetAECPriceEpisodePeriodisedValue(dasPaymentInfo.LearnerReferenceNumber, aecApprenticeshipPriceEpisodePeriodisedValues, Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName, period);
            }

            if (dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.Apprenticeship)
            {
                return GetAECPriceEpisodePeriodisedValue(dasPaymentInfo.LearnerReferenceNumber, aecApprenticeshipPriceEpisodePeriodisedValues, Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName, period);
            }

            return 0;
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
                                 x.AttributeName.CaseInsensitiveEquals(attributeName) &&
                                 GetDateFromPriceEpisodeIdentifier(x.PriceEpisodeIdentifier) >= Generics.BeginningOfYear)
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
