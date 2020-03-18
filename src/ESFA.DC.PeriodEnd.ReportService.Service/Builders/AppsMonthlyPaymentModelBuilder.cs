using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Utils;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class AppsMonthlyPaymentModelBuilder : IAppsMonthlyPaymentModelBuilder
    {
        private readonly HashSet<byte?> _fundingSourceLevyPayments = new HashSet<byte?>() { 1, 5 };
        private readonly HashSet<byte?> _fundingSourceCoInvestmentPayments = new HashSet<byte?>() { 2 };
        private readonly HashSet<byte?> _fundingSourceCoInvestmentDueFromEmployer = new HashSet<byte?>() { 3 };
        private readonly HashSet<byte?> _transactionTypesLevyPayments = new HashSet<byte?>() { 1, 2, 3 };
        private readonly HashSet<byte?> _transactionTypesCoInvestmentPayments = new HashSet<byte?>() { 1, 2, 3 };
        private readonly HashSet<byte?> _transactionTypesCoInvestmentDueFromEmployer = new HashSet<byte?>() { 1, 2, 3 };
        private readonly HashSet<byte?> _transactionTypesEmployerAdditionalPayments = new HashSet<byte?>() { 4, 6 };
        private readonly HashSet<byte?> _transactionTypesProviderAdditionalPayments = new HashSet<byte?>() { 5, 7 };
        private readonly HashSet<byte?> _transactionTypesApprenticeshipAdditionalPayments = new HashSet<byte?>() { 16 };
        private readonly HashSet<byte?> _transactionTypesEnglishAndMathsPayments = new HashSet<byte?>() { 13, 14 };
        private readonly HashSet<byte?> _transactionTypesLearningSupportPayments = new HashSet<byte?>() { 8, 9, 10, 11, 12, 15 };

        public IOrderedEnumerable<AppsMonthlyPaymentReportRowModel> BuildAppsMonthlyPaymentModelList(
            AppsMonthlyPaymentILRInfo ilrData,
            AppsMonthlyPaymentRulebaseInfo rulebaseData,
            AppsMonthlyPaymentDASInfo paymentsData,
            AppsMonthlyPaymentDasEarningsInfo earningsData,
            IDictionary<string, string> fcsData,
            IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> larsData)
        {
            // Payments are retrieved by UKPRN and AcademicYear in the payments provider code
            var reportRowModels = paymentsData.Payments?
                .GroupBy(r => new
                {
                    r.LearnerReferenceNumber,
                    r.LearnerUln,
                    r.LearningAimReference,
                    r.LearningStartDate,
                    r.LearningAimProgrammeType,
                    r.LearningAimStandardCode,
                    r.LearningAimFrameworkCode,
                    r.LearningAimPathwayCode,
                    r.ReportingAimFundingLineType,
                    r.PriceEpisodeIdentifier
                })
                .Select(g =>
                {
                    var aimSequenceNumber = GetPaymentAimSequenceNumber(g, earningsData);

                    return new AppsMonthlyPaymentReportRowModel
                    {
                        Ukprn = (int?)paymentsData.UkPrn,

                        // Br3 key columns
                        PaymentLearnerReferenceNumber = g.Key.LearnerReferenceNumber,
                        PaymentUniqueLearnerNumber = g.Key.LearnerUln,
                        PaymentLearningAimReference = g.Key.LearningAimReference,
                        PaymentLearningStartDate = g.Key.LearningStartDate,
                        PaymentProgrammeType = g.Key.LearningAimProgrammeType,
                        PaymentStandardCode = g.Key.LearningAimStandardCode,
                        PaymentFrameworkCode = g.Key.LearningAimFrameworkCode,
                        PaymentPathwayCode = g.Key.LearningAimPathwayCode,
                        PaymentFundingLineType = g.Key.ReportingAimFundingLineType,
                        PaymentPriceEpisodeIdentifier = g.Key.PriceEpisodeIdentifier,

                        PaymentApprenticeshipContractType = g.FirstOrDefault()?.ContractType,

                        // The PriceEpisodeStartDate isn't part of the Br3 grouping but is the last 10 characters of the PriceEpisodeIdentifier so will only have the one group row
                        PaymentPriceEpisodeStartDate = LookupPriceEpisodeStartDate(g.Key?.PriceEpisodeIdentifier),

                        // Official Sensitive is always empty so can be set as part of the grouping.
                        OfficialSensitive = string.Empty,

                        PaymentEarningEventAimSeqNumber = aimSequenceNumber,

                        // August payments - summed
                        AugustLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 1)).Sum(c => c.Amount ?? 0m),
                        AugustCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 1))
                            .Sum(c => c.Amount ?? 0m),
                        AugustCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 1))
                            .Sum(c => c.Amount ?? 0m),
                        AugustEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 1)).Sum(c => c.Amount ?? 0m),
                        AugustProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 1)).Sum(c => c.Amount ?? 0m),
                        AugustApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 1))
                                .Sum(c => c.Amount ?? 0m),
                        AugustEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 1))
                            .Sum(c => c.Amount ?? 0m),
                        AugustLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 1))
                            .Sum(c => c.Amount ?? 0m),

                        // September payments - summed
                        SeptemberLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 2)).Sum(c => c.Amount ?? 0m),
                        SeptemberCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 2))
                            .Sum(c => c.Amount ?? 0m),
                        SeptemberCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 2))
                            .Sum(c => c.Amount ?? 0m),
                        SeptemberEmployerAdditionalPayments = g
                            .Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 2)).Sum(c => c.Amount ?? 0m),
                        SeptemberProviderAdditionalPayments = g
                            .Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 2)).Sum(c => c.Amount ?? 0m),
                        SeptemberApprenticeAdditionalPayments = g
                            .Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 2)).Sum(c => c.Amount ?? 0m),
                        SeptemberEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 2)).Sum(c => c.Amount ?? 0m),
                        SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 2))
                            .Sum(c => c.Amount ?? 0m),

                        // October payments - summed
                        OctoberLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 3)).Sum(c => c.Amount ?? 0m),
                        OctoberCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 3))
                            .Sum(c => c.Amount ?? 0m),
                        OctoberCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 3))
                            .Sum(c => c.Amount ?? 0m),
                        OctoberEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 3)).Sum(c => c.Amount ?? 0m),
                        OctoberProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 3)).Sum(c => c.Amount ?? 0m),
                        OctoberApprenticeAdditionalPayments = g
                            .Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 3)).Sum(c => c.Amount ?? 0m),
                        OctoberEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 3))
                            .Sum(c => c.Amount ?? 0m),
                        OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 3))
                            .Sum(c => c.Amount ?? 0m),

                        // November payments - summed
                        NovemberLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 4)).Sum(c => c.Amount ?? 0m),
                        NovemberCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 4))
                            .Sum(c => c.Amount ?? 0m),
                        NovemberCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 4))
                            .Sum(c => c.Amount ?? 0m),
                        NovemberEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 4)).Sum(c => c.Amount ?? 0m),
                        NovemberProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 4)).Sum(c => c.Amount ?? 0m),
                        NovemberApprenticeAdditionalPayments = g
                            .Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 4)).Sum(c => c.Amount ?? 0m),
                        NovemberEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 4))
                            .Sum(c => c.Amount ?? 0m),
                        NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 4))
                            .Sum(c => c.Amount ?? 0m),

                        // December payments - summed
                        DecemberLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 5)).Sum(c => c.Amount ?? 0m),
                        DecemberCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 5))
                            .Sum(c => c.Amount ?? 0m),
                        DecemberCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 5))
                            .Sum(c => c.Amount ?? 0m),
                        DecemberEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 5)).Sum(c => c.Amount ?? 0m),
                        DecemberProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 5)).Sum(c => c.Amount ?? 0m),
                        DecemberApprenticeAdditionalPayments = g
                            .Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 5)).Sum(c => c.Amount ?? 0m),
                        DecemberEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 5))
                            .Sum(c => c.Amount ?? 0m),
                        DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 5))
                            .Sum(c => c.Amount ?? 0m),

                        // January payments - summed
                        JanuaryLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 6)).Sum(c => c.Amount ?? 0m),
                        JanuaryCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 6))
                            .Sum(c => c.Amount ?? 0m),
                        JanuaryCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 6))
                            .Sum(c => c.Amount ?? 0m),
                        JanuaryEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 6)).Sum(c => c.Amount ?? 0m),
                        JanuaryProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 6)).Sum(c => c.Amount ?? 0m),
                        JanuaryApprenticeAdditionalPayments = g
                            .Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 6)).Sum(c => c.Amount ?? 0m),
                        JanuaryEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 6))
                            .Sum(c => c.Amount ?? 0m),
                        JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 6))
                            .Sum(c => c.Amount ?? 0m),

                        // February payments - summed
                        FebruaryLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 7)).Sum(c => c.Amount ?? 0m),
                        FebruaryCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 7))
                            .Sum(c => c.Amount ?? 0m),
                        FebruaryCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 7))
                            .Sum(c => c.Amount ?? 0m),
                        FebruaryEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 7)).Sum(c => c.Amount ?? 0m),
                        FebruaryProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 7)).Sum(c => c.Amount ?? 0m),
                        FebruaryApprenticeAdditionalPayments = g
                            .Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 7)).Sum(c => c.Amount ?? 0m),
                        FebruaryEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 7))
                            .Sum(c => c.Amount ?? 0m),
                        FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 7))
                            .Sum(c => c.Amount ?? 0m),

                        // March payments - summed
                        MarchLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 8)).Sum(c => c.Amount ?? 0m),
                        MarchCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 8))
                            .Sum(c => c.Amount ?? 0m),
                        MarchCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 8))
                            .Sum(c => c.Amount ?? 0m),
                        MarchEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 8)).Sum(c => c.Amount ?? 0m),
                        MarchProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 8)).Sum(c => c.Amount ?? 0m),
                        MarchApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 8))
                                .Sum(c => c.Amount ?? 0m),
                        MarchEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 8))
                            .Sum(c => c.Amount ?? 0m),
                        MarchLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 8))
                            .Sum(c => c.Amount ?? 0m),

                        // April payments - summed
                        AprilLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 9)).Sum(c => c.Amount ?? 0m),
                        AprilCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 9))
                            .Sum(c => c.Amount ?? 0m),
                        AprilCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 9))
                            .Sum(c => c.Amount ?? 0m),
                        AprilEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 9)).Sum(c => c.Amount ?? 0m),
                        AprilProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 9)).Sum(c => c.Amount ?? 0m),
                        AprilApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 9))
                                .Sum(c => c.Amount ?? 0m),
                        AprilEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 9))
                            .Sum(c => c.Amount ?? 0m),
                        AprilLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 9))
                            .Sum(c => c.Amount ?? 0m),

                        // May payments - summed
                        MayLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 10)).Sum(c => c.Amount ?? 0m),
                        MayCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 10))
                            .Sum(c => c.Amount ?? 0m),
                        MayCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 10))
                            .Sum(c => c.Amount ?? 0m),
                        MayEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 10)).Sum(c => c.Amount ?? 0m),
                        MayProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 10)).Sum(c => c.Amount ?? 0m),
                        MayApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 10))
                                .Sum(c => c.Amount ?? 0m),
                        MayEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 10))
                            .Sum(c => c.Amount ?? 0m),
                        MayLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 10))
                            .Sum(c => c.Amount ?? 0m),

                        // June payments - summed
                        JuneLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 11)).Sum(c => c.Amount ?? 0m),
                        JuneCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 11))
                            .Sum(c => c.Amount ?? 0m),
                        JuneCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 11))
                            .Sum(c => c.Amount ?? 0m),
                        JuneEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 11)).Sum(c => c.Amount ?? 0m),
                        JuneProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 11)).Sum(c => c.Amount ?? 0m),
                        JuneApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 11))
                                .Sum(c => c.Amount ?? 0m),
                        JuneEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 11))
                            .Sum(c => c.Amount ?? 0m),
                        JuneLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 11))
                            .Sum(c => c.Amount ?? 0m),

                        // July payments - summed
                        JulyLevyPayments =
                            g.Where(p => PeriodLevyPaymentsTypePredicate(p, 12)).Sum(c => c.Amount ?? 0m),
                        JulyCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 12))
                            .Sum(c => c.Amount ?? 0m),
                        JulyCoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 12))
                            .Sum(c => c.Amount ?? 0m),
                        JulyEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 12)).Sum(c => c.Amount ?? 0m),
                        JulyProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 12)).Sum(c => c.Amount ?? 0m),
                        JulyApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 12))
                                .Sum(c => c.Amount ?? 0m),
                        JulyEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 12))
                            .Sum(c => c.Amount ?? 0m),
                        JulyLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 12))
                            .Sum(c => c.Amount ?? 0m),

                        // R13 payments - summed
                        R13LevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 13)).Sum(c => c.Amount ?? 0m),
                        R13CoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 13))
                            .Sum(c => c.Amount ?? 0m),
                        R13CoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 13))
                            .Sum(c => c.Amount ?? 0m),
                        R13EmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 13)).Sum(c => c.Amount ?? 0m),
                        R13ProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 13)).Sum(c => c.Amount ?? 0m),
                        R13ApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 13))
                                .Sum(c => c.Amount ?? 0m),
                        R13EnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 13))
                            .Sum(c => c.Amount ?? 0m),
                        R13LearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 13))
                            .Sum(c => c.Amount ?? 0m),

                        // R14 payments - summed
                        R14LevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 14)).Sum(c => c.Amount ?? 0m),
                        R14CoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 14))
                            .Sum(c => c.Amount ?? 0m),
                        R14CoInvestmentDueFromEmployerPayments = g
                            .Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 14))
                            .Sum(c => c.Amount ?? 0m),
                        R14EmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 14)).Sum(c => c.Amount ?? 0m),
                        R14ProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 14)).Sum(c => c.Amount ?? 0m),
                        R14ApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 14))
                                .Sum(c => c.Amount ?? 0m),
                        R14EnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 14))
                            .Sum(c => c.Amount ?? 0m),
                        R14LearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 14))
                            .Sum(c => c.Amount ?? 0m),

                        // Total payments
                        TotalLevyPayments = g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount ?? 0m),
                        TotalCoInvestmentPayments = g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount ?? 0m),
                        TotalCoInvestmentDueFromEmployerPayments =
                            g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount ?? 0m),
                        TotalEmployerAdditionalPayments =
                            g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount ?? 0m),
                        TotalProviderAdditionalPayments =
                            g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount ?? 0m),
                        TotalApprenticeAdditionalPayments =
                            g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount ?? 0m),
                        TotalEnglishAndMathsPayments = g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount ?? 0m),
                        TotalLearningSupportDisadvantageAndFrameworkUpliftPayments =
                            g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount ?? 0m)
                    };
                }).ToList();

            // populate the appsMonthlyPaymentModel payment related fields
            if (reportRowModels != null)
            {
                foreach (var reportRowModel in reportRowModels)
                {
                    reportRowModel.LarsLearningDeliveryLearningAimTitle = LookupAimTitle(reportRowModel.PaymentLearningAimReference, larsData);
                    reportRowModel.FcsContractContractAllocationContractAllocationNumber = LookupContractAllocationNumber(reportRowModel.PaymentFundingLineType, fcsData);

                    var ilrLearnerForThisPayment = LookupLearner(reportRowModel.PaymentLearnerReferenceNumber, ilrData);
                    reportRowModel.LearnerCampusIdentifier = ilrLearnerForThisPayment?.CampId;

                    reportRowModel.ProviderSpecifiedLearnerMonitoringA = LookupProvSpecLearnMon(ilrLearnerForThisPayment?.ProviderSpecLearnerMonitorings, Generics.ProviderSpecifiedLearnerMonitoringA);
                    reportRowModel.ProviderSpecifiedLearnerMonitoringB = LookupProvSpecLearnMon(ilrLearnerForThisPayment?.ProviderSpecLearnerMonitorings, Generics.ProviderSpecifiedLearnerMonitoringB);

                    var ilrLearningDeliveryForThisPayment = LookupLearningDelivery(reportRowModel, ilrLearnerForThisPayment);
                    reportRowModel.LearningDeliveryOriginalLearningStartDate = ilrLearningDeliveryForThisPayment?.OrigLearnStartDate;
                    reportRowModel.LearningDeliveryLearningPlannedEndDate = ilrLearningDeliveryForThisPayment?.LearnPlanEndDate;
                    reportRowModel.LearningDeliveryCompletionStatus = ilrLearningDeliveryForThisPayment?.CompStatus;
                    reportRowModel.LearningDeliveryLearningActualEndDate = ilrLearningDeliveryForThisPayment?.LearnActEndDate;
                    reportRowModel.LearningDeliveryAchievementDate = ilrLearningDeliveryForThisPayment?.AchDate;
                    reportRowModel.LearningDeliveryOutcome = ilrLearningDeliveryForThisPayment?.Outcome;
                    reportRowModel.LearningDeliveryAimType = ilrLearningDeliveryForThisPayment?.AimType;
                    reportRowModel.LearningDeliverySoftwareSupplierAimIdentifier = ilrLearningDeliveryForThisPayment?.SwSupAimId;
                    reportRowModel.LearningDeliveryEndPointAssessmentOrganisation = ilrLearningDeliveryForThisPayment?.EpaOrgId;
                    reportRowModel.LearningDeliverySubContractedOrPartnershipUkprn = ilrLearningDeliveryForThisPayment?.PartnerUkprn;

                    var ldmFamArray = LookupLearningDeliveryLdmFams(ilrLearningDeliveryForThisPayment?.LearningDeliveryFams, Generics.LearningDeliveryFAMCodeLDM);
                    reportRowModel.LearningDeliveryFamTypeLearningDeliveryMonitoringA = ldmFamArray?[0]?.LearnDelFAMCode;
                    reportRowModel.LearningDeliveryFamTypeLearningDeliveryMonitoringB = ldmFamArray?[1]?.LearnDelFAMCode;
                    reportRowModel.LearningDeliveryFamTypeLearningDeliveryMonitoringC = ldmFamArray?[2]?.LearnDelFAMCode;
                    reportRowModel.LearningDeliveryFamTypeLearningDeliveryMonitoringD = ldmFamArray?[3]?.LearnDelFAMCode;
                    reportRowModel.LearningDeliveryFamTypeLearningDeliveryMonitoringE = ldmFamArray?[4]?.LearnDelFAMCode;
                    reportRowModel.LearningDeliveryFamTypeLearningDeliveryMonitoringF = ldmFamArray?[5]?.LearnDelFAMCode;

                    reportRowModel.ProviderSpecifiedDeliveryMonitoringA = LookupProvSpecDelMon(ilrLearningDeliveryForThisPayment?.ProviderSpecDeliveryMonitorings, Generics.ProviderSpecifiedDeliveryMonitoringA);
                    reportRowModel.ProviderSpecifiedDeliveryMonitoringB = LookupProvSpecDelMon(ilrLearningDeliveryForThisPayment?.ProviderSpecDeliveryMonitorings, Generics.ProviderSpecifiedDeliveryMonitoringB);
                    reportRowModel.ProviderSpecifiedDeliveryMonitoringC = LookupProvSpecDelMon(ilrLearningDeliveryForThisPayment?.ProviderSpecDeliveryMonitorings, Generics.ProviderSpecifiedDeliveryMonitoringC);
                    reportRowModel.ProviderSpecifiedDeliveryMonitoringD = LookupProvSpecDelMon(ilrLearningDeliveryForThisPayment?.ProviderSpecDeliveryMonitorings, Generics.ProviderSpecifiedDeliveryMonitoringD);

                    var aecPriceEpisodeForThisPayment = LookupAecPriceEpisode(reportRowModel, rulebaseData);
                    reportRowModel.RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier = aecPriceEpisodeForThisPayment?.PriceEpisodeAgreeId;
                    reportRowModel.RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate = aecPriceEpisodeForThisPayment?.PriceEpisodeActualEndDateIncEPA;

                    var aecLearningDeliveryForThisPayment = LookupAecLearningDelivery(reportRowModel, rulebaseData, ilrLearningDeliveryForThisPayment?.AimSeqNumber);
                    reportRowModel.RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim = aecLearningDeliveryForThisPayment?.PlannedNumOnProgInstalm;

                    var ilrLearnerEmploymentStatus = LookupLearnerEmploymentStatus(ilrLearnerForThisPayment?.LearnerEmploymentStatus, ilrLearningDeliveryForThisPayment?.LearnStartDate);
                    reportRowModel.LearnerEmploymentStatusEmployerId = ilrLearnerEmploymentStatus?.EmpdId;
                    reportRowModel.LearnerEmploymentStatus = ilrLearnerEmploymentStatus?.EmpStat;
                    reportRowModel.LearnerEmploymentStatusDate = ilrLearnerEmploymentStatus?.DateEmpStatApp;

                    reportRowModel.AugustTotalPayments = GetAugPaymentTypeTotals(reportRowModel);
                    reportRowModel.SeptemberTotalPayments = GetSepPaymentTypeTotals(reportRowModel);
                    reportRowModel.OctoberTotalPayments = GetOctPaymentTypeTotals(reportRowModel);
                    reportRowModel.NovemberTotalPayments = GetNovPaymentTypeTotals(reportRowModel);
                    reportRowModel.DecemberTotalPayments = GetDecPaymentTypeTotals(reportRowModel);
                    reportRowModel.JanuaryTotalPayments = GetJanPaymentTypeTotals(reportRowModel);
                    reportRowModel.FebruaryTotalPayments = GetFebPaymentTypeTotals(reportRowModel);
                    reportRowModel.MarchTotalPayments = GetMarPaymentTypeTotals(reportRowModel);
                    reportRowModel.AprilTotalPayments = GetAprPaymentTypeTotals(reportRowModel);
                    reportRowModel.MayTotalPayments = GetMayPaymentTypeTotals(reportRowModel);
                    reportRowModel.JuneTotalPayments = GetJunPaymentTypeTotals(reportRowModel);
                    reportRowModel.JulyTotalPayments = GetJulPaymentTypeTotals(reportRowModel);
                    reportRowModel.R13TotalPayments = GetR13PaymentTypeTotals(reportRowModel);
                    reportRowModel.R14TotalPayments = GetR14PaymentTypeTotals(reportRowModel);

                    reportRowModel.TotalLevyPayments = GetLevyPaymentTotalForAllPeriods(reportRowModel);
                    reportRowModel.TotalCoInvestmentPayments = GetCoInvestmentPaymentTotalForAllPeriods(reportRowModel);
                    reportRowModel.TotalCoInvestmentDueFromEmployerPayments = GetCoInvestmentDueFromEmployerPaymentTotalForAllPeriods(reportRowModel);
                    reportRowModel.TotalEmployerAdditionalPayments = GetEmployerAdditionalPaymentTotalForAllPeriods(reportRowModel);
                    reportRowModel.TotalProviderAdditionalPayments = GetProviderAdditionalPaymentTotalForAllPeriods(reportRowModel);
                    reportRowModel.TotalApprenticeAdditionalPayments = GetApprenticeAdditionalPaymentTotalForAllPeriods(reportRowModel);
                    reportRowModel.TotalEnglishAndMathsPayments = GetEnglishAndMathsPaymentTotalForAllPeriods(reportRowModel);
                    reportRowModel.TotalLearningSupportDisadvantageAndFrameworkUpliftPayments = GetLSDPaymentTotalForAllPeriods(reportRowModel);
                    reportRowModel.TotalPayments = CalculateTotalPayments(reportRowModel);
                }
            }

            return reportRowModels?.OrderBy(p => p.PaymentLearnerReferenceNumber);
        }

        public string LookupPriceEpisodeStartDate(string priceEpisodeIdentifier)
        {
            return (!string.IsNullOrEmpty(priceEpisodeIdentifier) && priceEpisodeIdentifier.Length > 10)
                ? priceEpisodeIdentifier.Substring(priceEpisodeIdentifier.Length - 10, 10)
                : string.Empty;
        }

        public byte? GetPaymentAimSequenceNumber(IEnumerable<AppsMonthlyPaymentDasPaymentModel> g, AppsMonthlyPaymentDasEarningsInfo earningsData)
        {
            byte? aimSequenceNumber = null;
            List<AppsMonthlyPaymentDasPaymentModel> group = g.ToList();

            var paymentEarningEventIds = group
                .Where(a => a.EarningEventId != new Guid("00000000-0000-0000-0000-000000000000"))
                .Select(a => a.EarningEventId)
                .ToList();

            var earningsEvents = earningsData?.Earnings?
                .Where(x => paymentEarningEventIds.Contains(x.EventId))
                .ToList();

            var distinctEarningEventAimSequenceNumbers = earningsEvents?.Select(e => e.LearningAimSequenceNumber).Distinct().ToList();
            var distinctEarningEventAimSequenceNumbersCount = distinctEarningEventAimSequenceNumbers?.Count();

            if (distinctEarningEventAimSequenceNumbersCount > 1)
            {
                var latestPayment = group.OrderByDescending(p => p.AcademicYear).ThenByDescending(p => p.CollectionPeriod)
                    .ThenByDescending(p => p.DeliveryPeriod).First();

                aimSequenceNumber = earningsEvents?.FirstOrDefault(e => e.EventId == latestPayment.EarningEventId)
                    ?.LearningAimSequenceNumber;
            }
            else if (distinctEarningEventAimSequenceNumbersCount == 1)
            {
                aimSequenceNumber = distinctEarningEventAimSequenceNumbers.First();
            }

            return aimSequenceNumber;
        }

        public string LookupAimTitle(string aimReferenceNumber, IEnumerable<AppsMonthlyPaymentLarsLearningDeliveryInfo> larsData)
        {
            var larsInfo = larsData?.FirstOrDefault(x => x != null && x.LearnAimRef.CaseInsensitiveEquals(aimReferenceNumber));

            return larsInfo?.LearningAimTitle;
        }

        public string LookupContractAllocationNumber(string fundingLineType, IDictionary<string, string> fcsData)
        {
            string fundingStreamPeriodCode = Utils.GetFundingStreamPeriodForFundingLineType(fundingLineType);

            return fcsData?.GetValueOrDefault(fundingStreamPeriodCode ?? string.Empty, Generics.NoContract);
        }

        public AppsMonthlyPaymentLearnerModel LookupLearner(string learnerReferenceNumber, AppsMonthlyPaymentILRInfo ilrData)
        {
            return ilrData?.Learners?.Where(x => x != null && x.LearnRefNumber.CaseInsensitiveEquals(learnerReferenceNumber)).FirstOrDefault();
        }

        public string LookupProvSpecLearnMon(IEnumerable<AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo> providerSpecLearnerMonitorings, string provSpecMonOccur)
        {
            return providerSpecLearnerMonitorings?.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(provSpecMonOccur) && x.ProvSpecLearnMonOccur.Equals(provSpecMonOccur))?.ProvSpecLearnMon;
        }

        public AppsMonthlyPaymentLearningDeliveryModel LookupLearningDelivery(AppsMonthlyPaymentReportRowModel reportRowModel, AppsMonthlyPaymentLearnerModel ilrLearnerForThisPayment)
        {
            return ilrLearnerForThisPayment?.LearningDeliveries?.FirstOrDefault(ld => ld != null &&
                reportRowModel != null &&
                ld.Ukprn == reportRowModel.Ukprn &&
                ld.LearnRefNumber.CaseInsensitiveEquals(reportRowModel?.PaymentLearnerReferenceNumber) &&
                ld.LearnAimRef.CaseInsensitiveEquals(reportRowModel?.PaymentLearningAimReference) &&
                ld.LearnStartDate == reportRowModel?.PaymentLearningStartDate &&
                ld.ProgType == reportRowModel?.PaymentProgrammeType &&
                ld.StdCode == reportRowModel?.PaymentStandardCode &&
                ld.FworkCode == reportRowModel?.PaymentFrameworkCode &&
                ld.PwayCode == reportRowModel?.PaymentPathwayCode);
        }

        public AppsMonthlyPaymentLearningDeliveryFAMInfo[] LookupLearningDeliveryLdmFams(IEnumerable<AppsMonthlyPaymentLearningDeliveryFAMInfo> ldmFams, string learnDelFamType)
        {
            return ldmFams?.Where(fam => fam.LearnDelFAMType.CaseInsensitiveEquals(learnDelFamType)).ToFixedLengthArray(6);
        }

        public string LookupProvSpecDelMon(IEnumerable<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo> providerSpecDeliveryMonitorings, string provSpecDelMonOccur)
        {
            return providerSpecDeliveryMonitorings?.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(provSpecDelMonOccur) && x.ProvSpecDelMonOccur.Equals(provSpecDelMonOccur))?.ProvSpecDelMon;
        }

        public AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo LookupAecPriceEpisode(AppsMonthlyPaymentReportRowModel reportRowModel, AppsMonthlyPaymentRulebaseInfo rulebaseData)
        {
            return rulebaseData?.AecApprenticeshipPriceEpisodeInfoList
                .Where(x => x != null &&
                       x.Ukprn == reportRowModel?.Ukprn &&
                       x.LearnRefNumber.CaseInsensitiveEquals(reportRowModel?.PaymentLearnerReferenceNumber) &&
                       x.PriceEpisodeIdentifier.CaseInsensitiveEquals(reportRowModel?.PaymentPriceEpisodeIdentifier))
                .OrderByDescending(x => x.EpisodeStartDate)
                .FirstOrDefault();
        }

        public AppsMonthlyPaymentAECLearningDeliveryInfo LookupAecLearningDelivery(AppsMonthlyPaymentReportRowModel reportRowModel, AppsMonthlyPaymentRulebaseInfo rulebaseData, byte? learningDeliveryAimSeqNumber)
        {
            return rulebaseData?.AecLearningDeliveryInfoList?.FirstOrDefault(x => x != null &&
                        learningDeliveryAimSeqNumber != null &&
                        x.Ukprn == reportRowModel?.Ukprn &&
                        x.LearnRefNumber.CaseInsensitiveEquals(reportRowModel?.PaymentLearnerReferenceNumber) &&
                        x.AimSequenceNumber == learningDeliveryAimSeqNumber &&
                        x.LearnAimRef == reportRowModel?.PaymentLearningAimReference);
        }

        public AppsMonthlyPaymentLearnerEmploymentStatusInfo LookupLearnerEmploymentStatus(IEnumerable<AppsMonthlyPaymentLearnerEmploymentStatusInfo> employmentStatusData, DateTime? learningDeliveryLearnStartDate)
        {
            return employmentStatusData
                .Where(les => les?.DateEmpStatApp <= learningDeliveryLearnStartDate)
                .OrderByDescending(les => les.DateEmpStatApp)
                .FirstOrDefault();
        }

        public decimal GetAugPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.AugustLevyPayments,
                reportRowModel?.AugustCoInvestmentPayments,
                reportRowModel?.AugustEmployerAdditionalPayments,
                reportRowModel?.AugustProviderAdditionalPayments,
                reportRowModel?.AugustApprenticeAdditionalPayments,
                reportRowModel?.AugustEnglishAndMathsPayments,
                reportRowModel?.AugustLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetSepPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.SeptemberLevyPayments,
                reportRowModel?.SeptemberCoInvestmentPayments,
                reportRowModel?.SeptemberEmployerAdditionalPayments,
                reportRowModel?.SeptemberProviderAdditionalPayments,
                reportRowModel?.SeptemberApprenticeAdditionalPayments,
                reportRowModel?.SeptemberEnglishAndMathsPayments,
                reportRowModel?.SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetOctPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.OctoberLevyPayments,
                reportRowModel?.OctoberCoInvestmentPayments,
                reportRowModel?.OctoberEmployerAdditionalPayments,
                reportRowModel?.OctoberProviderAdditionalPayments,
                reportRowModel?.OctoberApprenticeAdditionalPayments,
                reportRowModel?.OctoberEnglishAndMathsPayments,
                reportRowModel?.OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetNovPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.NovemberLevyPayments,
                reportRowModel?.NovemberCoInvestmentPayments,
                reportRowModel?.NovemberEmployerAdditionalPayments,
                reportRowModel?.NovemberProviderAdditionalPayments,
                reportRowModel?.NovemberApprenticeAdditionalPayments,
                reportRowModel?.NovemberEnglishAndMathsPayments,
                reportRowModel?.NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetDecPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.DecemberLevyPayments,
                reportRowModel?.DecemberCoInvestmentPayments,
                reportRowModel?.DecemberEmployerAdditionalPayments,
                reportRowModel?.DecemberProviderAdditionalPayments,
                reportRowModel?.DecemberApprenticeAdditionalPayments,
                reportRowModel?.DecemberEnglishAndMathsPayments,
                reportRowModel?.DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetJanPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.JanuaryLevyPayments,
                reportRowModel?.JanuaryCoInvestmentPayments,
                reportRowModel?.JanuaryEmployerAdditionalPayments,
                reportRowModel?.JanuaryProviderAdditionalPayments,
                reportRowModel?.JanuaryApprenticeAdditionalPayments,
                reportRowModel?.JanuaryEnglishAndMathsPayments,
                reportRowModel?.JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetFebPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.FebruaryLevyPayments,
                reportRowModel?.FebruaryCoInvestmentPayments,
                reportRowModel?.FebruaryEmployerAdditionalPayments,
                reportRowModel?.FebruaryProviderAdditionalPayments,
                reportRowModel?.FebruaryApprenticeAdditionalPayments,
                reportRowModel?.FebruaryEnglishAndMathsPayments,
                reportRowModel?.FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetMarPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.MarchLevyPayments,
                reportRowModel?.MarchCoInvestmentPayments,
                reportRowModel?.MarchEmployerAdditionalPayments,
                reportRowModel?.MarchProviderAdditionalPayments,
                reportRowModel?.MarchApprenticeAdditionalPayments,
                reportRowModel?.MarchEnglishAndMathsPayments,
                reportRowModel?.MarchLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetAprPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.AprilLevyPayments,
                reportRowModel?.AprilCoInvestmentPayments,
                reportRowModel?.AprilEmployerAdditionalPayments,
                reportRowModel?.AprilProviderAdditionalPayments,
                reportRowModel?.AprilApprenticeAdditionalPayments,
                reportRowModel?.AprilEnglishAndMathsPayments,
                reportRowModel?.AprilLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetMayPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.MayLevyPayments,
                reportRowModel?.MayCoInvestmentPayments,
                reportRowModel?.MayEmployerAdditionalPayments,
                reportRowModel?.MayProviderAdditionalPayments,
                reportRowModel?.MayApprenticeAdditionalPayments,
                reportRowModel?.MayEnglishAndMathsPayments,
                reportRowModel?.MayLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetJunPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.JuneLevyPayments,
                reportRowModel?.JuneCoInvestmentPayments,
                reportRowModel?.JuneEmployerAdditionalPayments,
                reportRowModel?.JuneProviderAdditionalPayments,
                reportRowModel?.JuneApprenticeAdditionalPayments,
                reportRowModel?.JuneEnglishAndMathsPayments,
                reportRowModel?.JuneLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetJulPaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.JulyLevyPayments,
                reportRowModel?.JulyCoInvestmentPayments,
                reportRowModel?.JulyEmployerAdditionalPayments,
                reportRowModel?.JulyProviderAdditionalPayments,
                reportRowModel?.JulyApprenticeAdditionalPayments,
                reportRowModel?.JulyEnglishAndMathsPayments,
                reportRowModel?.JulyLearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetR13PaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.R13LevyPayments,
                reportRowModel?.R13CoInvestmentPayments,
                reportRowModel?.R13EmployerAdditionalPayments,
                reportRowModel?.R13ProviderAdditionalPayments,
                reportRowModel?.R13ApprenticeAdditionalPayments,
                reportRowModel?.R13EnglishAndMathsPayments,
                reportRowModel?.R13LearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetR14PaymentTypeTotals(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPaymentTypeTotals(
                reportRowModel?.R14LevyPayments,
                reportRowModel?.R14CoInvestmentPayments,
                reportRowModel?.R14EmployerAdditionalPayments,
                reportRowModel?.R14ProviderAdditionalPayments,
                reportRowModel?.R14ApprenticeAdditionalPayments,
                reportRowModel?.R14EnglishAndMathsPayments,
                reportRowModel?.R14LearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetPaymentTypeTotals(
            decimal? levy = 0m,
            decimal? coin = 0m,
            decimal? empl = 0m,
            decimal? prov = 0m,
            decimal? apps = 0m,
            decimal? engm = 0m,
            decimal? lsdu = 0m)
        {
            return (levy ?? 0m) +
                   (coin ?? 0m) +
                   (empl ?? 0m) +
                   (prov ?? 0m) +
                   (apps ?? 0m) +
                   (engm ?? 0m) +
                   (lsdu ?? 0m);
        }

        public decimal GetLevyPaymentTotalForAllPeriods(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPeriodPaymentTotals(
                reportRowModel?.AugustLevyPayments,
                reportRowModel?.SeptemberLevyPayments,
                reportRowModel?.OctoberLevyPayments,
                reportRowModel?.NovemberLevyPayments,
                reportRowModel?.DecemberLevyPayments,
                reportRowModel?.JanuaryLevyPayments,
                reportRowModel?.FebruaryLevyPayments,
                reportRowModel?.MarchLevyPayments,
                reportRowModel?.AprilLevyPayments,
                reportRowModel?.MayLevyPayments,
                reportRowModel?.JuneLevyPayments,
                reportRowModel?.JulyLevyPayments,
                reportRowModel?.R13LevyPayments,
                reportRowModel?.R14LevyPayments);
        }

        public decimal GetCoInvestmentPaymentTotalForAllPeriods(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPeriodPaymentTotals(
                reportRowModel?.AugustCoInvestmentPayments,
                reportRowModel?.SeptemberCoInvestmentPayments,
                reportRowModel?.OctoberCoInvestmentPayments,
                reportRowModel?.NovemberCoInvestmentPayments,
                reportRowModel?.DecemberCoInvestmentPayments,
                reportRowModel?.JanuaryCoInvestmentPayments,
                reportRowModel?.FebruaryCoInvestmentPayments,
                reportRowModel?.MarchCoInvestmentPayments,
                reportRowModel?.AprilCoInvestmentPayments,
                reportRowModel?.MayCoInvestmentPayments,
                reportRowModel?.JuneCoInvestmentPayments,
                reportRowModel?.JulyCoInvestmentPayments,
                reportRowModel?.R13CoInvestmentPayments,
                reportRowModel?.R14CoInvestmentPayments);
        }

        public decimal GetCoInvestmentDueFromEmployerPaymentTotalForAllPeriods(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPeriodPaymentTotals(
                reportRowModel?.AugustCoInvestmentDueFromEmployerPayments,
                reportRowModel?.SeptemberCoInvestmentDueFromEmployerPayments,
                reportRowModel?.OctoberCoInvestmentDueFromEmployerPayments,
                reportRowModel?.NovemberCoInvestmentDueFromEmployerPayments,
                reportRowModel?.DecemberCoInvestmentDueFromEmployerPayments,
                reportRowModel?.JanuaryCoInvestmentDueFromEmployerPayments,
                reportRowModel?.FebruaryCoInvestmentDueFromEmployerPayments,
                reportRowModel?.MarchCoInvestmentDueFromEmployerPayments,
                reportRowModel?.AprilCoInvestmentDueFromEmployerPayments,
                reportRowModel?.MayCoInvestmentDueFromEmployerPayments,
                reportRowModel?.JuneCoInvestmentDueFromEmployerPayments,
                reportRowModel?.JulyCoInvestmentDueFromEmployerPayments,
                reportRowModel?.R13CoInvestmentDueFromEmployerPayments,
                reportRowModel?.R14CoInvestmentDueFromEmployerPayments);
        }

        public decimal GetEmployerAdditionalPaymentTotalForAllPeriods(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPeriodPaymentTotals(
                reportRowModel?.AugustEmployerAdditionalPayments,
                reportRowModel?.SeptemberEmployerAdditionalPayments,
                reportRowModel?.OctoberEmployerAdditionalPayments,
                reportRowModel?.NovemberEmployerAdditionalPayments,
                reportRowModel?.DecemberEmployerAdditionalPayments,
                reportRowModel?.JanuaryEmployerAdditionalPayments,
                reportRowModel?.FebruaryEmployerAdditionalPayments,
                reportRowModel?.MarchEmployerAdditionalPayments,
                reportRowModel?.AprilEmployerAdditionalPayments,
                reportRowModel?.MayEmployerAdditionalPayments,
                reportRowModel?.JuneEmployerAdditionalPayments,
                reportRowModel?.JulyEmployerAdditionalPayments,
                reportRowModel?.R13EmployerAdditionalPayments,
                reportRowModel?.R14EmployerAdditionalPayments);
        }

        public decimal GetProviderAdditionalPaymentTotalForAllPeriods(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPeriodPaymentTotals(
                reportRowModel?.AugustProviderAdditionalPayments,
                reportRowModel?.SeptemberProviderAdditionalPayments,
                reportRowModel?.OctoberProviderAdditionalPayments,
                reportRowModel?.NovemberProviderAdditionalPayments,
                reportRowModel?.DecemberProviderAdditionalPayments,
                reportRowModel?.JanuaryProviderAdditionalPayments,
                reportRowModel?.FebruaryProviderAdditionalPayments,
                reportRowModel?.MarchProviderAdditionalPayments,
                reportRowModel?.AprilProviderAdditionalPayments,
                reportRowModel?.MayProviderAdditionalPayments,
                reportRowModel?.JuneProviderAdditionalPayments,
                reportRowModel?.JulyProviderAdditionalPayments,
                reportRowModel?.R13ProviderAdditionalPayments,
                reportRowModel?.R14ProviderAdditionalPayments);
        }

        public decimal GetApprenticeAdditionalPaymentTotalForAllPeriods(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPeriodPaymentTotals(
                reportRowModel?.AugustApprenticeAdditionalPayments,
                reportRowModel?.SeptemberApprenticeAdditionalPayments,
                reportRowModel?.OctoberApprenticeAdditionalPayments,
                reportRowModel?.NovemberApprenticeAdditionalPayments,
                reportRowModel?.DecemberApprenticeAdditionalPayments,
                reportRowModel?.JanuaryApprenticeAdditionalPayments,
                reportRowModel?.FebruaryApprenticeAdditionalPayments,
                reportRowModel?.MarchApprenticeAdditionalPayments,
                reportRowModel?.AprilApprenticeAdditionalPayments,
                reportRowModel?.MayApprenticeAdditionalPayments,
                reportRowModel?.JuneApprenticeAdditionalPayments,
                reportRowModel?.JulyApprenticeAdditionalPayments,
                reportRowModel?.R13ApprenticeAdditionalPayments,
                reportRowModel?.R14ApprenticeAdditionalPayments);
        }

        public decimal GetEnglishAndMathsPaymentTotalForAllPeriods(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPeriodPaymentTotals(
                reportRowModel?.AugustEnglishAndMathsPayments,
                reportRowModel?.SeptemberEnglishAndMathsPayments,
                reportRowModel?.OctoberEnglishAndMathsPayments,
                reportRowModel?.NovemberEnglishAndMathsPayments,
                reportRowModel?.DecemberEnglishAndMathsPayments,
                reportRowModel?.JanuaryEnglishAndMathsPayments,
                reportRowModel?.FebruaryEnglishAndMathsPayments,
                reportRowModel?.MarchEnglishAndMathsPayments,
                reportRowModel?.AprilEnglishAndMathsPayments,
                reportRowModel?.MayEnglishAndMathsPayments,
                reportRowModel?.JuneEnglishAndMathsPayments,
                reportRowModel?.JulyEnglishAndMathsPayments,
                reportRowModel?.R13EnglishAndMathsPayments,
                reportRowModel?.R14EnglishAndMathsPayments);
        }

        public decimal GetLSDPaymentTotalForAllPeriods(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return GetPeriodPaymentTotals(
                reportRowModel?.AugustLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.MarchLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.AprilLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.MayLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.JuneLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.JulyLearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.R13LearningSupportDisadvantageAndFrameworkUpliftPayments,
                reportRowModel?.R14LearningSupportDisadvantageAndFrameworkUpliftPayments);
        }

        public decimal GetPeriodPaymentTotals(
            decimal? aug = 0m,
            decimal? sep = 0m,
            decimal? oct = 0m,
            decimal? nov = 0m,
            decimal? dec = 0m,
            decimal? jan = 0m,
            decimal? feb = 0m,
            decimal? mar = 0m,
            decimal? apr = 0m,
            decimal? may = 0m,
            decimal? jun = 0m,
            decimal? jul = 0m,
            decimal? r13 = 0m,
            decimal? r14 = 0m)
        {
            return aug ?? 0m +
                   sep ?? 0m +
                   oct ?? 0m +
                   nov ?? 0m +
                   dec ?? 0m +
                   jan ?? 0m +
                   feb ?? 0m +
                   mar ?? 0m +
                   apr ?? 0m +
                   may ?? 0m +
                   jun ?? 0m +
                   jul ?? 0m +
                   r13 ?? 0m +
                   r14 ?? 0m;
        }

        public decimal CalculateTotalPayments(AppsMonthlyPaymentReportRowModel reportRowModel)
        {
            return (reportRowModel?.AugustTotalPayments ?? 0m) +
                   (reportRowModel?.SeptemberTotalPayments ?? 0m) +
                   (reportRowModel?.OctoberTotalPayments ?? 0m) +
                   (reportRowModel?.NovemberTotalPayments ?? 0m) +
                   (reportRowModel?.DecemberTotalPayments ?? 0m) +
                   (reportRowModel?.JanuaryTotalPayments ?? 0m) +
                   (reportRowModel?.FebruaryTotalPayments ?? 0m) +
                   (reportRowModel?.MarchTotalPayments ?? 0m) +
                   (reportRowModel?.AprilTotalPayments ?? 0m) +
                   (reportRowModel?.MayTotalPayments ?? 0m) +
                   (reportRowModel?.JuneTotalPayments ?? 0m) +
                   (reportRowModel?.JulyTotalPayments ?? 0m) +
                   (reportRowModel?.R13TotalPayments ?? 0m) +
                   (reportRowModel?.R14TotalPayments ?? 0m);
        }

        //------------------------------------------------------------------------------------------------------
        // Levy Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is a Levy payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Levy payment, otherwise false.</returns>
        private bool PeriodLevyPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalLevyPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalLevyPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == 1920 &&
                   payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                   _fundingSourceLevyPayments.Contains(payment.FundingSource) &&
                   _transactionTypesLevyPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // CoInvestment Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is a CoInvestment payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a CoInvestment payment, otherwise false.</returns>
        private bool PeriodCoInvestmentPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalCoInvestmentPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalCoInvestmentPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == 1920 &&
                          payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _fundingSourceCoInvestmentPayments.Contains(payment.FundingSource) &&
                          _transactionTypesCoInvestmentPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // CoInvestment Payments Due From Employer Payment Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payments is a CoInvestment payment due from the Employer.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a CoInvestment due from the employer payment, otherwise false.</returns>
        private bool PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(payment);

            return result;
        }

        private bool TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == 1920 &&
                          payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _fundingSourceCoInvestmentDueFromEmployer.Contains(payment.FundingSource) &&
                         _transactionTypesCoInvestmentDueFromEmployer.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // Employer Additional Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is an Employer additional payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if an Employer additional payment, otherwise false.</returns>
        private bool PeriodEmployerAdditionalPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalEmployerAdditionalPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalEmployerAdditionalPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == 1920 &&
                          payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _transactionTypesEmployerAdditionalPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // Provider Additional Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is Provider additional payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Provider additional payment, otherwise false.</returns>
        private bool PeriodProviderAdditionalPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalProviderAdditionalPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalProviderAdditionalPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == 1920 &&
                          payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _transactionTypesProviderAdditionalPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // Apprenticeship Additional Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is an Apprenticeship additional payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if an Apprenticeship additional payment, otherwise false.</returns>
        private bool PeriodApprenticeAdditionalPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalProviderApprenticeshipAdditionalPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalProviderApprenticeshipAdditionalPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == 1920 &&
                   payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                   _transactionTypesApprenticeshipAdditionalPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // English and Maths Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is an English and Maths payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if an English and Maths payment.</returns>
        private bool PeriodEnglishAndMathsPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalEnglishAndMathsPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalEnglishAndMathsPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == 1920 &&
                          !payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _transactionTypesEnglishAndMathsPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // Learning Support, Disadvantage and Framework Uplift Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is a Learning Support, Disadvantage and Framework Uplift payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if aLearning Support, Disadvantage and Framework Uplift payments.</returns>
        private bool PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                   TotalLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == 1920 &&
                           ((payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                           _transactionTypesLearningSupportPayments.Contains(payment.TransactionType)) ||
                           (!payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) && payment.TransactionType == 15));

            return result;
        }
    }
}
