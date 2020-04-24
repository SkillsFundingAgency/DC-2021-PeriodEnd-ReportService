using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Constants;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Reports.AppsAdditionalPaymentsReport.Model;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using AppsAdditionalPaymentExtendedPaymentModel = ESFA.DC.PeriodEnd.ReportService.Legacy.Reports.AppsAdditionalPaymentsReport.Model.AppsAdditionalPaymentExtendedPaymentModel;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Reports.AppsAdditionalPaymentsReport
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

        public IEnumerable<AppsAdditionalPaymentsModel> BuildModel(
            IList<AppsAdditionalPaymentLearnerInfo> appsAdditionalPaymentIlrInfo,
            IList<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> rulebasePriceEpisodes,
            IList<AECLearningDeliveryInfo> rulebaseLearningDeliveries,
            IList<DASPaymentInfo> appsAdditionalPaymentDasPaymentsInfo,
            IDictionary<long, string> legalNameDictionary,
            int ukPrn)
        {
            var learnerDictionary = appsAdditionalPaymentIlrInfo.ToDictionary(l => l.LearnRefNumber, l => l, StringComparer.OrdinalIgnoreCase);

            var priceEpisodesDictionary = rulebasePriceEpisodes
                .Where(x => x != null)
                .GroupBy(pel => pel.LearnRefNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key,
                    g => g
                        .GroupBy(gea => gea.AimSeqNumber)
                        .ToDictionary(gi => gi.Key, gi => gi.ToArray()));

            // Create an extended payments model that includes the payment related ILR data
            var extendedPayments = BuildAdditionalPaymentsExtendedPaymentsModel(
                learnerDictionary,
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
            var groups = extendedPayments
                .GroupBy(x => new AppsAdditionalPaymentRecordKey(
                    x.PaymentLearnerReferenceNumber,
                    x.PaymentUniqueLearnerNumber,
                    x.PaymentLearningStartDate,
                    x.PaymentLearningAimFundingLineType,
                    x.PaymentTypeOfAdditionalPayment,
                    x.AppsServiceEmployerName,
                    x.ilrEmployerIdentifier
                ), AppsAdditionalPaymentRecordKey.AppsAdditionalPaymentRecordKeyComparer)
                .ToList();

            var earningsData = BuildGroupedEarningsValuesByPeriod(groups, learnerDictionary, priceEpisodesDictionary);

            return groups
                .Select(g =>
                {
                    var academicYearPayments = g.Where(p => p.PaymentAcademicYear == Generics.AcademicYear).ToList();

                    var groupedAcademicYear = academicYearPayments
                        .GroupBy(p => p.PaymentCollectionPeriod)
                        .ToDictionary(pg => pg.Key, pg => pg.ToList());

                    var firstModel = g.FirstOrDefault();

                    return new AppsAdditionalPaymentsModel
                    {
                        Ukprn = ukPrn,
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
                        AugustEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 1),
                        AugustR01Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 1),
                        SeptemberEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 2),
                        SeptemberR02Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 2),
                        OctoberEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 3),
                        OctoberR03Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 3),
                        NovemberEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 4),
                        NovemberR04Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 4),
                        DecemberEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 5),
                        DecemberR05Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 5),
                        JanuaryEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 6),
                        JanuaryR06Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 6),
                        FebruaryEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 7),
                        FebruaryR07Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 7),
                        MarchEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 8),
                        MarchR08Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 8),
                        AprilEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 9),
                        AprilR09Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 9),
                        MayEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 10),
                        MayR10Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 10),
                        JuneEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 11),
                        JuneR11Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 11),
                        JulyEarnings = GetEarningsForKeyAndPeriod(earningsData, g.Key, 12),
                        JulyR12Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 12),
                        R13Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 13),
                        R14Payments = GetPeriodPaymentsTotalForPeriod(groupedAcademicYear, 14),

                        // Annual totals
                        TotalEarnings = earningsData.GetValueOrDefault(g.Key)?.Values.Sum() ?? 0,
                        TotalPaymentsYearToDate = academicYearPayments.Where(p => p.PaymentAmount != null && _applicablePaymentTypes.Contains(p.PaymentTypeOfAdditionalPayment)).Sum(c => c.PaymentAmount.Value),
                    };
                })
                .OrderBy(o => o.LearnerReferenceNumber)
                .ToList();
        }

        public Dictionary<AppsAdditionalPaymentRecordKey, Dictionary<int, decimal>> BuildGroupedEarningsValuesByPeriod(
            IEnumerable<IGrouping<AppsAdditionalPaymentRecordKey, AppsAdditionalPaymentExtendedPaymentModel>> groups,
            IDictionary<string, AppsAdditionalPaymentLearnerInfo> learnerDictionary,
            Dictionary<string, Dictionary<int?, AECApprenticeshipPriceEpisodePeriodisedValuesInfo[]>> priceEpisodesDictionary)
        {
            return groups.ToDictionary(
                g => g.Key,
                v =>
                {
                    var learnRefNumber = v.Key.PaymentLearnerReferenceNumber;

                    var learner = learnerDictionary.GetValueOrDefault(learnRefNumber);

                    var learningDeliveries = learner?.LearningDeliveries?
                        .Where(x => v.Any(dasPaymentInfo =>
                            x != null &&
                            x.ProgType == dasPaymentInfo.PaymentLearningAimProgrammeType &&
                            x.StdCode == dasPaymentInfo.PaymentLearningAimStandardCode &&
                            x.FworkCode == dasPaymentInfo.PaymentLearningAimFrameworkCode &&
                            x.PwayCode == dasPaymentInfo.PaymentLearningAimPathwayCode &&
                            x.LearnRefNumber.CaseInsensitiveEquals(dasPaymentInfo.PaymentLearnerReferenceNumber) &&
                            x.LearnAimRef.CaseInsensitiveEquals(dasPaymentInfo.PaymentLearningAimReference) &&
                            x.LearnStartDate == dasPaymentInfo.PaymentLearningStartDate));

                    if (learningDeliveries != null)
                    {
                        var priceEpisodesForLearningDeliveries = learningDeliveries.Select(ld =>
                            priceEpisodesDictionary
                                .GetValueOrDefault(learnRefNumber)?
                                .GetValueOrDefault(ld.AimSeqNumber))?
                            .Where(pe => pe != null)
                            .ToArray();

                        return new Dictionary<int, decimal>()
                        {
                            [1] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period1, v.Key.PaymentTypeOfAdditionalPayment),
                            [2] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period2, v.Key.PaymentTypeOfAdditionalPayment),
                            [3] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period3, v.Key.PaymentTypeOfAdditionalPayment),
                            [4] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period4, v.Key.PaymentTypeOfAdditionalPayment),
                            [5] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period5, v.Key.PaymentTypeOfAdditionalPayment),
                            [6] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period6, v.Key.PaymentTypeOfAdditionalPayment),
                            [7] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period7, v.Key.PaymentTypeOfAdditionalPayment),
                            [8] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period8, v.Key.PaymentTypeOfAdditionalPayment),
                            [9] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period9, v.Key.PaymentTypeOfAdditionalPayment),
                            [10] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period10, v.Key.PaymentTypeOfAdditionalPayment),
                            [11] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period11, v.Key.PaymentTypeOfAdditionalPayment),
                            [12] = SumPriceEpisodesByPeriodForPaymentType(priceEpisodesForLearningDeliveries, pe => pe.Period12, v.Key.PaymentTypeOfAdditionalPayment),
                        };
                    }

                    return new Dictionary<int, decimal>();
                },
                AppsAdditionalPaymentRecordKey.AppsAdditionalPaymentRecordKeyComparer);
        }

        public decimal SumPriceEpisodesByPeriodForPaymentType(AECApprenticeshipPriceEpisodePeriodisedValuesInfo[][] priceEpisodes, Func<AECApprenticeshipPriceEpisodePeriodisedValuesInfo, decimal?> periodSelector, string paymentType)
        {
            return priceEpisodes
                .Sum(p => p.Where(pe => periodSelector(pe) != null && EarningsAttributeFilter(pe, paymentType))
                    .Sum(pe => periodSelector(pe).Value));
        }

        public bool EarningsAttributeFilter(AECApprenticeshipPriceEpisodePeriodisedValuesInfo periodisedValues, string paymentType)
        {
            if (paymentType.CaseInsensitiveEquals(EmployerPaymentType) &&
                (periodisedValues.AttributeName.CaseInsensitiveEquals(Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName) ||
                    periodisedValues.AttributeName.CaseInsensitiveEquals(Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName)))
            {
                return true;
            }

            if (paymentType.CaseInsensitiveEquals(ProviderPaymentType) &&
                (periodisedValues.AttributeName.CaseInsensitiveEquals(Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName) ||
                    periodisedValues.AttributeName.CaseInsensitiveEquals(Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName)))
            {
                return true;
            }

            if (paymentType == ApprenticePaymentType && periodisedValues.AttributeName == Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName)
            {
                return true;
            }

            return false;
        }

        public decimal GetEarningsForKeyAndPeriod(Dictionary<AppsAdditionalPaymentRecordKey, Dictionary<int, decimal>> earnings, AppsAdditionalPaymentRecordKey key, int period)
        {
            return earnings
                .GetValueOrDefault(key)
                .GetValueOrDefault(period, 0);
        }

        public IEnumerable<AppsAdditionalPaymentExtendedPaymentModel> BuildAdditionalPaymentsExtendedPaymentsModel(
            IDictionary<string, AppsAdditionalPaymentLearnerInfo> learnerDictionary,
            IList<AECLearningDeliveryInfo> rulebaseLearningDeliveries,
            IList<DASPaymentInfo> appsAdditionalPaymentDasPaymentsInfo,
            IDictionary<long, string> legalNameDictionary)
        {
            // Create a new payment model which includes the related data from the ILR
            return appsAdditionalPaymentDasPaymentsInfo
                .Where(p => p != null)
                .Select(dasPaymentInfo =>
                {
                    AppsAdditionalPaymentLearnerInfo learner = null;
                    AppsAdditionalPaymentLearningDeliveryInfo learningDelivery = null;
                    AECLearningDeliveryInfo aecLearningDelivery = null;

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
                        ilrEmployerIdentifier = aecLearningDelivery != null ? GetEmployerIdentifier(aecLearningDelivery, dasPaymentInfo.TransactionType) : null,

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
                    };
                });
        }

        private decimal GetPeriodPaymentsTotalForPeriod(IDictionary<byte, List<AppsAdditionalPaymentExtendedPaymentModel>> models, byte period)
        {
            return models.GetValueOrDefault(period)?.Where(p => p.PaymentAmount.HasValue && _applicablePaymentTypes.Contains(p.PaymentTypeOfAdditionalPayment)).Sum(c => c.PaymentAmount.Value) ?? 0;
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
