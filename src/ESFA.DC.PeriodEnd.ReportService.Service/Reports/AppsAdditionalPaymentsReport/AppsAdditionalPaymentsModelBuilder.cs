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

        public IEnumerable<AppsAdditionalPaymentExtendedPaymentModel> BuildAdditionalPaymentsExtendedPaymentsModel(
            IList<AppsAdditionalPaymentLearnerInfo> appsAdditionalPaymentIlrInfo,
            IList<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> rulebasePriceEpisodes,
            IList<AECLearningDeliveryInfo> rulebaseLearningDeliveries,
            IList<DASPaymentInfo> appsAdditionalPaymentDasPaymentsInfo,
            IDictionary<long, string> legalNameDictionary)
        {
            var learnerDictionary = appsAdditionalPaymentIlrInfo.ToDictionary(l => l.LearnRefNumber, l => l, StringComparer.OrdinalIgnoreCase);

            var priceEpisodesDictionary = rulebasePriceEpisodes
                .Where(x => x != null)
                .GroupBy(pel => pel.LearnRefNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key,
                    g => g
                        .GroupBy(gea => gea.AimSeqNumber)
                        .ToDictionary(gi => gi.Key, gi => gi.ToArray()));

            // Create a new payment model which includes the related data from the ILR
            return appsAdditionalPaymentDasPaymentsInfo
                .Where(p => p != null)
                .Select(dasPaymentInfo =>
                {
                    AppsAdditionalPaymentLearnerInfo learner;
                    AppsAdditionalPaymentLearningDeliveryInfo learningDelivery;
                    AECLearningDeliveryInfo aecLearningDelivery;
                    AECApprenticeshipPriceEpisodePeriodisedValuesInfo[] aecApprenticeshipPriceEpisodePeriodisedValues;

                    // lookup the related reference data for this payment
                    learner = learnerDictionary.GetValueOrDefault(dasPaymentInfo?.LearnerReferenceNumber);

                    if (learner != null)
                    {
                        learningDelivery = learner?.LearningDeliveries?.FirstOrDefault(x =>
                            x != null &&
                            x.ProgType == dasPaymentInfo.LearningAimProgrammeType &&
                            x.StdCode == dasPaymentInfo.LearningAimStandardCode &&
                            x.FworkCode == dasPaymentInfo.LearningAimFrameworkCode &&
                            x.PwayCode == dasPaymentInfo.LearningAimPathwayCode &&
                            x.LearnRefNumber.CaseInsensitiveEquals(dasPaymentInfo.LearnerReferenceNumber) &&
                            x.LearnAimRef.CaseInsensitiveEquals(dasPaymentInfo.LearningAimReference) &&
                            x.LearnStartDate == dasPaymentInfo.LearningStartDate);

                        if (learningDelivery != null)
                        {
                            aecLearningDelivery = rulebaseLearningDeliveries?.FirstOrDefault(x =>
                                x != null &&
                                x.UKPRN == learningDelivery.UKPRN &&
                                x.LearnRefNumber.CaseInsensitiveEquals(learningDelivery.LearnRefNumber) &&
                                x.AimSeqNumber == learningDelivery.AimSeqNumber);

                            aecApprenticeshipPriceEpisodePeriodisedValues =
                                priceEpisodesDictionary.GetValueOrDefault(learningDelivery.LearnRefNumber)?
                                    .GetValueOrDefault(learningDelivery.AimSeqNumber)
                                ?? Array.Empty<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>();
                        }
                    }

                    // copy this payment's fields to the new extended payment model
                    return new AppsAdditionalPaymentExtendedPaymentModel
                    {
                        // copy the reporting grouping fields
                        PaymentLearnerReferenceNumber = dasPaymentInfo.LearnerReferenceNumber,
                        PaymentUniqueLearnerNumber = dasPaymentInfo.LearnerUln,
                        PaymentLearningStartDate = dasPaymentInfo.LearningStartDate,
                        PaymentLearningAimFundingLineType = dasPaymentInfo.LearningAimFundingLineType,
                        PaymentTypeOfAdditionalPayment = GetTypeOfAdditionalPayment(dasPaymentInfo.TransactionType),
                        AppsServiceEmployerName = GetAppServiceEmployerName(dasPaymentInfo, legalNameDictionary),
                        ilrEmployerIdentifier = GetEmployerIdentifier(aecLearningDelivery, dasPaymentInfo.TransactionType),

                        // copy the remaining payment fields
                        PaymentLearningAimProgrammeType = dasPaymentInfo.LearningAimProgrammeType,
                        PaymentLearningAimStandardCode = dasPaymentInfo.LearningAimStandardCode,
                        PaymentLearningAimFrameworkCode = dasPaymentInfo.LearningAimFrameworkCode,
                        PaymentLearningAimPathwayCode = dasPaymentInfo.LearningAimPathwayCode,
                        PaymentLearningAimReference = dasPaymentInfo.LearningAimReference,
                        PaymentContractType = dasPaymentInfo.ContractType,
                        PaymentFundingSource = dasPaymentInfo.FundingSource,
                        PaymentTransactionType = dasPaymentInfo.TransactionType,
                        PaymentAcademicYear = dasPaymentInfo.AcademicYear,
                        PaymentCollectionPeriod = dasPaymentInfo.CollectionPeriod,
                        PaymentDeliveryPeriod = dasPaymentInfo.DeliveryPeriod,
                        PaymentAmount = dasPaymentInfo.Amount,

                        // copy the ilr fields
                        ProviderSpecifiedLearnerMonitoringA = GetProviderSpecMonitor(learner, Generics.ProviderSpecifiedLearnerMonitoringA),
                        ProviderSpecifiedLearnerMonitoringB = GetProviderSpecMonitor(learner, Generics.ProviderSpecifiedLearnerMonitoringB),
                        EarningAmount = GetMonthlyEarnings(dasPaymentInfo, aecApprenticeshipPriceEpisodePeriodisedValues, dasPaymentInfo.CollectionPeriod)
                    };
                });
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
            return learner?.ProviderSpecLearnerMonitorings.FirstOrDefault(x => x.ProvSpecLearnMonOccur.CaseInsensitiveEquals(providerSpecifiedLearnerMonitoring))?.ProvSpecLearnMon;
        }

        private decimal GetMonthlyEarnings(
            byte transactionType,
            ICollection<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> aecApprenticeshipPriceEpisodePeriodisedValues,
            byte period)
        {
            if (period >= 1 && period <= 12)
            {
                if (transactionType == Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive ||
                    transactionType == Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive)
                {
                    return GetAECPriceEpisodePeriodisedValue(
                        aecApprenticeshipPriceEpisodePeriodisedValues,
                        new[]
                        {
                            Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName,
                            Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName
                        }, period);
                }

                if (transactionType == Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive ||
                    transactionType == Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive)
                {
                    return GetAECPriceEpisodePeriodisedValue(
                        aecApprenticeshipPriceEpisodePeriodisedValues,
                        new[]
                        {
                            Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName,
                            Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName
                        }, period);
                }

                if (transactionType == Constants.DASPayments.TransactionType.Apprenticeship)
                {
                    return GetAECPriceEpisodePeriodisedValue(aecApprenticeshipPriceEpisodePeriodisedValues, new[] {Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName}, period);
                }
            }

            return 0;
        }

        private decimal GetAECPriceEpisodePeriodisedValue(
            ICollection<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> aecApprenticeshipPriceEpisodePeriodisedValues,
            ICollection<string> attributeNames,
            byte period)
        {
            return aecApprenticeshipPriceEpisodePeriodisedValues
                             .Where(x =>
                                 x != null &&
                                 attributeNames.Contains(x.AttributeName, StringComparer.OrdinalIgnoreCase))
                             .Sum(z => z.Periods[period - 1]) ?? 0;
        }

        private string GetEmployerIdentifier(AECLearningDeliveryInfo aecLearningDeliveryInfo, byte transactionType)
        {
            if (aecLearningDeliveryInfo != null)
            {
                if (transactionType == Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive) // 4
                {
                    return aecLearningDeliveryInfo.LearnDelEmpIdFirstAdditionalPaymentThreshold?.ToString();
                }

                if (transactionType == Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive) //6
                {
                    return aecLearningDeliveryInfo.LearnDelEmpIdSecondAdditionalPaymentThreshold?.ToString();
                }

                return Generics.NotAvailable;
            }

            return null;
        }
    }
}
