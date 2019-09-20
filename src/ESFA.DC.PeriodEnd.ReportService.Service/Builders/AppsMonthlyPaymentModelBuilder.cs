using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Interface.Utils;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class AppsMonthlyPaymentModelBuilder : IAppsMonthlyPaymentModelBuilder
    {
        private const string ZPROG001 = "ZPROG001";

        private AppsMonthlyPaymentILRInfo _appsMonthlyPaymentIlrInfo;
        private AppsMonthlyPaymentRulebaseInfo _appsMonthlyPaymentRulebaseInfo;
        private AppsMonthlyPaymentDASInfo _appsMonthlyPaymentDasInfo;
        private AppsMonthlyPaymentDasEarningsInfo _appsMonthlyPaymentDasEarningsInfo;
        private AppsMonthlyPaymentFcsInfo _appsMonthlyPaymentFcsInfo;

        private IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo>
            _appsMonthlyPaymentLarsLearningDeliveryInfoList;

        private readonly string[] _collectionPeriods =
        {
            "1920-R01",
            "1920-R02",
            "1920-R03",
            "1920-R04",
            "1920-R05",
            "1920-R06",
            "1920-R07",
            "1920-R08",
            "1920-R09",
            "1920-R10",
            "1920-R11",
            "1920-R12",
            "1920-R13",
            "1920-R14"
        };

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

        private readonly HashSet<byte?> _transactionTypesLearningSupportPayments = new HashSet<byte?>() { 8, 9, 10, 11, 12 };

        public IReadOnlyList<AppsMonthlyPaymentModel> BuildAppsMonthlyPaymentModelList(
            AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo,
            AppsMonthlyPaymentRulebaseInfo appsMonthlyPaymentRulebaseInfo,
            AppsMonthlyPaymentDASInfo appsMonthlyPaymentDasInfo,
            AppsMonthlyPaymentDasEarningsInfo appsMonthlyPaymentDasEarningsInfo,
            AppsMonthlyPaymentFcsInfo appsMonthlyPaymentFcsInfo,
            IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> appsMonthlyPaymentLarsLearningDeliveryInfoList)
        {
            // cache the passed in data for use in the private 'Get' methods
            _appsMonthlyPaymentIlrInfo = appsMonthlyPaymentIlrInfo;
            _appsMonthlyPaymentRulebaseInfo = appsMonthlyPaymentRulebaseInfo;
            _appsMonthlyPaymentDasInfo = appsMonthlyPaymentDasInfo;
            _appsMonthlyPaymentDasEarningsInfo = appsMonthlyPaymentDasEarningsInfo;
            _appsMonthlyPaymentFcsInfo = appsMonthlyPaymentFcsInfo;
            _appsMonthlyPaymentLarsLearningDeliveryInfoList = appsMonthlyPaymentLarsLearningDeliveryInfoList;

            // this variable is the final report and is the return value of this method.
            List<AppsMonthlyPaymentModel> appsMonthlyPaymentModelList = null;
            try
            {
                /*
                --------------------------------------------------
                From Apps Monthly Payment Report spec version 3.3
                --------------------------------------------------

                BR1 – Applicable Records

                This report shows rows of calculated payment data related to funding appsMonthlyPaymentModel 36, and associated ILR data.

                ------------------------------------------------------------------------------------------------------------------------------------------
                *** There should be a new row on the report where the data is different for any of the following fields in the Payments2.Payment table:***
                ------------------------------------------------------------------------------------------------------------------------------------------
                • LearnerReferenceNumber
                • LearnerUln
                • LearningAimReference
                • LearningStartDate
                • LearningAimProgrammeType
                • LearningAimStandardCode
                • LearningAimFrameworkCode
                • LearningAimPathwayCode
                • ReportingAimFundingLineType
                • PriceEpisodeIdentifier(note that only programme aims(LearningAimReference = ZPROG001) have PriceEpisodeIdentifiers; maths and English aims do not)

                ----------------------------------------------------------------------------------------------------------------------------------------
                *** Where these fields are identical, multiple payments should be displayed in the appropriate monthly field and summed as necessary ***
                ----------------------------------------------------------------------------------------------------------------------------------------

                There may be multiple price episodes for an aim.  Only price episodes with a start date in the current funding year should be included on this report.
                Note that English and maths aims do not have price episodes, so there should be just one row per aim.
                */
                appsMonthlyPaymentModelList = appsMonthlyPaymentDasInfo.Payments?
                    .Where(p => p.AcademicYear == 1920)
                    .GroupBy(r => new
                    {
                        r.Ukprn,
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
                    .Select(g => new AppsMonthlyPaymentModel
                    {
                        Ukprn = g.Key.Ukprn,

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

                        PaymentApprenticeshipContractType = g?.FirstOrDefault().ContractType,

                        // The PriceEpisodeStartDate isn't part of the Br3 grouping but is the last 10 characters of the PriceEpisodeIdentifier
                        // so will only have the one group row
                        PaymentPriceEpisodeStartDate = (!string.IsNullOrEmpty(g.Key?.PriceEpisodeIdentifier) && g.Key?.PriceEpisodeIdentifier.Length > 10) ? g.Key?.PriceEpisodeIdentifier.Substring(g.Key.PriceEpisodeIdentifier.Length - 10, 10) : string.Empty,

                        // Official Sensitive is always empty so can be set as part of the grouping.
                        OfficialSensitive = string.Empty,

                        // August payments - summed
                        AugustLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 1)).Sum(c => c.Amount),
                        AugustCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 1)).Sum(c => c.Amount),
                        AugustCoInvestmentDueFromEmployerPayments = g.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 1)).Sum(c => c.Amount),
                        AugustEmployerAdditionalPayments = g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 1)).Sum(c => c.Amount),
                        AugustProviderAdditionalPayments = g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 1)).Sum(c => c.Amount),
                        AugustApprenticeAdditionalPayments = g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 1)).Sum(c => c.Amount),
                        AugustEnglishAndMathsPayments = g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 1)).Sum(c => c.Amount),
                        AugustLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p => PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 1)).Sum(c => c.Amount),

                        // September payments - summed
                        SeptemberLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 2)).Sum(c => c.Amount),
                        SeptemberCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 2))
                            .Sum(c => c.Amount),
                        SeptemberCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 2))
                            .Sum(c => c.Amount),
                        SeptemberEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 2)).Sum(c => c.Amount),
                        SeptemberProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 2)).Sum(c => c.Amount),
                        SeptemberApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 2)).Sum(c => c.Amount),
                        SeptemberEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 2)).Sum(c => c.Amount),
                        SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 2))
                            .Sum(c => c.Amount),

                        // October payments - summed
                        OctoberLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 3)).Sum(c => c.Amount),
                        OctoberCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 3))
                            .Sum(c => c.Amount),
                        OctoberCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 3))
                            .Sum(c => c.Amount),
                        OctoberEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 3)).Sum(c => c.Amount),
                        OctoberProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 3)).Sum(c => c.Amount),
                        OctoberApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 3)).Sum(c => c.Amount),
                        OctoberEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 3)).Sum(c => c.Amount),
                        OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 3))
                            .Sum(c => c.Amount),

                        // November payments - summed
                        NovemberLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 4)).Sum(c => c.Amount),
                        NovemberCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 4))
                            .Sum(c => c.Amount),
                        NovemberCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 4))
                            .Sum(c => c.Amount),
                        NovemberEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 4)).Sum(c => c.Amount),
                        NovemberProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 4)).Sum(c => c.Amount),
                        NovemberApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 4)).Sum(c => c.Amount),
                        NovemberEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 4)).Sum(c => c.Amount),
                        NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 4))
                            .Sum(c => c.Amount),

                        // December payments - summed
                        DecemberLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 5)).Sum(c => c.Amount),
                        DecemberCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 5))
                            .Sum(c => c.Amount),
                        DecemberCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 5))
                            .Sum(c => c.Amount),
                        DecemberEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 5)).Sum(c => c.Amount),
                        DecemberProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 5)).Sum(c => c.Amount),
                        DecemberApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 5)).Sum(c => c.Amount),
                        DecemberEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 5)).Sum(c => c.Amount),
                        DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 5))
                            .Sum(c => c.Amount),

                        // January payments - summed
                        JanuaryLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 6)).Sum(c => c.Amount),
                        JanuaryCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 6))
                            .Sum(c => c.Amount),
                        JanuaryCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 6))
                            .Sum(c => c.Amount),
                        JanuaryEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 6)).Sum(c => c.Amount),
                        JanuaryProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 6)).Sum(c => c.Amount),
                        JanuaryApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 6)).Sum(c => c.Amount),
                        JanuaryEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 6)).Sum(c => c.Amount),
                        JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 6))
                            .Sum(c => c.Amount),

                        // February payments - summed
                        FebruaryLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 7)).Sum(c => c.Amount),
                        FebruaryCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 7))
                            .Sum(c => c.Amount),
                        FebruaryCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 7))
                            .Sum(c => c.Amount),
                        FebruaryEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 7)).Sum(c => c.Amount),
                        FebruaryProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 7)).Sum(c => c.Amount),
                        FebruaryApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 7)).Sum(c => c.Amount),
                        FebruaryEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 7)).Sum(c => c.Amount),
                        FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 7))
                            .Sum(c => c.Amount),

                        // March payments - summed
                        MarchLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 8)).Sum(c => c.Amount),
                        MarchCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 8))
                            .Sum(c => c.Amount),
                        MarchCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 8))
                            .Sum(c => c.Amount),
                        MarchEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 8)).Sum(c => c.Amount),
                        MarchProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 8)).Sum(c => c.Amount),
                        MarchApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 8)).Sum(c => c.Amount),
                        MarchEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 8)).Sum(c => c.Amount),
                        MarchLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 8))
                            .Sum(c => c.Amount),

                        // April payments - summed
                        AprilLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 9)).Sum(c => c.Amount),
                        AprilCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 9))
                            .Sum(c => c.Amount),
                        AprilCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 9))
                            .Sum(c => c.Amount),
                        AprilEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 9)).Sum(c => c.Amount),
                        AprilProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 9)).Sum(c => c.Amount),
                        AprilApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 9)).Sum(c => c.Amount),
                        AprilEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 9)).Sum(c => c.Amount),
                        AprilLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 9))
                            .Sum(c => c.Amount),

                        // May payments - summed
                        MayLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 10)).Sum(c => c.Amount),
                        MayCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 10))
                            .Sum(c => c.Amount),
                        MayCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 10))
                            .Sum(c => c.Amount),
                        MayEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 10)).Sum(c => c.Amount),
                        MayProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 10)).Sum(c => c.Amount),
                        MayApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 10)).Sum(c => c.Amount),
                        MayEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 10)).Sum(c => c.Amount),
                        MayLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 10))
                            .Sum(c => c.Amount),

                        // June payments - summed
                        JuneLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 11)).Sum(c => c.Amount),
                        JuneCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 11))
                            .Sum(c => c.Amount),
                        JuneCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 11))
                            .Sum(c => c.Amount),
                        JuneEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 11)).Sum(c => c.Amount),
                        JuneProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 11)).Sum(c => c.Amount),
                        JuneApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 11)).Sum(c => c.Amount),
                        JuneEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 11)).Sum(c => c.Amount),
                        JuneLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 11))
                            .Sum(c => c.Amount),

                        // July payments - summed
                        JulyLevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 12)).Sum(c => c.Amount),
                        JulyCoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 12))
                            .Sum(c => c.Amount),
                        JulyCoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 12))
                            .Sum(c => c.Amount),
                        JulyEmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 12)).Sum(c => c.Amount),
                        JulyProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 12)).Sum(c => c.Amount),
                        JulyApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 12)).Sum(c => c.Amount),
                        JulyEnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 12)).Sum(c => c.Amount),
                        JulyLearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 12))
                            .Sum(c => c.Amount),

                        // R13 payments - summed
                        R13LevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 13)).Sum(c => c.Amount),
                        R13CoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 13))
                            .Sum(c => c.Amount),
                        R13CoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 13))
                            .Sum(c => c.Amount),
                        R13EmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 13)).Sum(c => c.Amount),
                        R13ProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 13)).Sum(c => c.Amount),
                        R13ApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 13)).Sum(c => c.Amount),
                        R13EnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 13)).Sum(c => c.Amount),
                        R13LearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 13))
                            .Sum(c => c.Amount),

                        // R14 payments - summed
                        R14LevyPayments = g.Where(p => PeriodLevyPaymentsTypePredicate(p, 14)).Sum(c => c.Amount),
                        R14CoInvestmentPayments = g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, 14))
                            .Sum(c => c.Amount),
                        R14CoInvestmentDueFromEmployerPayments = g.Where(p =>
                                PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, 14))
                            .Sum(c => c.Amount),
                        R14EmployerAdditionalPayments =
                            g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, 14)).Sum(c => c.Amount),
                        R14ProviderAdditionalPayments =
                            g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, 14)).Sum(c => c.Amount),
                        R14ApprenticeAdditionalPayments =
                            g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, 14)).Sum(c => c.Amount),
                        R14EnglishAndMathsPayments =
                            g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, 14)).Sum(c => c.Amount),
                        R14LearningSupportDisadvantageAndFrameworkUpliftPayments = g.Where(p =>
                                PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, 14))
                            .Sum(c => c.Amount),

                        // Total payments
                        TotalLevyPayments = g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount),
                        TotalCoInvestmentPayments = g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount),
                        TotalCoInvestmentDueFromEmployerPayments =
                            g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount),
                        TotalEmployerAdditionalPayments = g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount),
                        TotalProviderAdditionalPayments = g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount),
                        TotalApprenticeAdditionalPayments = g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount),
                        TotalEnglishAndMathsPayments = g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount),
                        TotalLearningSupportDisadvantageAndFrameworkUpliftPayments =
                            g.Where(TotalLevyPaymentsTypePredicate).Sum(c => c.Amount)
                    }).ToList();

                // populate the appsMonthlyPaymentModel payment related fields
                if (appsMonthlyPaymentModelList != null)
                {
                    foreach (var appsMonthlyPaymentModel in appsMonthlyPaymentModelList)
                    {
                        //------------------------------------------------------------------------------------------------------------------
                        // Aim Sequence Number processing
                        // Get the Earning Event Id for the latest payment in the group of payments
                        // (there may be multiple payment rows that were rolled up into a single row as part of the grouping)
                        //------------------------------------------------------------------------------------------------------------------
                        if (_appsMonthlyPaymentDasInfo != null)
                        {
                            var paymentEarningEventId = _appsMonthlyPaymentDasInfo?.Payments
                                .Where(x => x?.Ukprn == appsMonthlyPaymentModel?.Ukprn &&
                                            x.LearnerReferenceNumber.CaseInsensitiveEquals(appsMonthlyPaymentModel?.PaymentLearnerReferenceNumber) &&
                                            x?.LearnerUln == appsMonthlyPaymentModel?.PaymentUniqueLearnerNumber &&
                                            x.LearningAimReference.CaseInsensitiveEquals(appsMonthlyPaymentModel?.PaymentLearningAimReference) &&
                                            x?.LearningStartDate == appsMonthlyPaymentModel?.PaymentLearningStartDate &&
                                            (x?.LearningAimProgrammeType == null || x?.LearningAimProgrammeType == appsMonthlyPaymentModel?.PaymentProgrammeType) &&
                                            (x?.LearningAimStandardCode == null || x?.LearningAimStandardCode == appsMonthlyPaymentModel?.PaymentStandardCode) &&
                                            (x?.LearningAimFrameworkCode == null || x?.LearningAimFrameworkCode == appsMonthlyPaymentModel?.PaymentFrameworkCode) &&
                                            (x?.LearningAimPathwayCode == null || x?.LearningAimPathwayCode == appsMonthlyPaymentModel?.PaymentPathwayCode) &&
                                            x.ReportingAimFundingLineType.CaseInsensitiveEquals(appsMonthlyPaymentModel?.PaymentFundingLineType) &&
                                            x?.PriceEpisodeIdentifier == appsMonthlyPaymentModel?.PaymentPriceEpisodeIdentifier)
                                .OrderByDescending(x => x?.AcademicYear)
                                .ThenByDescending(x => x?.CollectionPeriod)
                                .ThenByDescending(x => x?.DeliveryPeriod)
                                .FirstOrDefault()?.EarningEventId;

                            if (paymentEarningEventId != null)
                            {
                                // get the matching sequence number for this earning event id from the Earning Event table
                                appsMonthlyPaymentModel.PaymentEarningEventAimSeqNumber = _appsMonthlyPaymentDasEarningsInfo?.Earnings
                                   ?.SingleOrDefault(x => x?.EventId == paymentEarningEventId)?.LearningAimSequenceNumber;
                            }
                        }

                        //--------------------------------------------------------------------------------------------------
                        // process the LARS fields
                        //--------------------------------------------------------------------------------------------------
                        if (_appsMonthlyPaymentLarsLearningDeliveryInfoList != null)
                        {
                            var larsInfo = _appsMonthlyPaymentLarsLearningDeliveryInfoList?.SingleOrDefault(x =>
                                x.LearnAimRef.CaseInsensitiveEquals(appsMonthlyPaymentModel
                                    ?.PaymentLearningAimReference));

                            // populate the LARS fields in the appsMonthlyPaymentModel payment.
                            if (larsInfo != null)
                            {
                                appsMonthlyPaymentModel.LarsLearningDeliveryLearningAimTitle = larsInfo?.LearningAimTitle;
                            }
                        }

                        //--------------------------------------------------------------------------------------------------
                        // process the FCS Contract fields
                        //--------------------------------------------------------------------------------------------------
                        string fundingStreamPeriodCode =
                            Utils.GetFundingStreamPeriodForFundingLineType(appsMonthlyPaymentModel?
                                .PaymentFundingLineType);

                        if (!string.IsNullOrEmpty(fundingStreamPeriodCode) &&
                            _appsMonthlyPaymentFcsInfo.Contracts != null)
                        {
                            var contractAllocationNumber = _appsMonthlyPaymentFcsInfo.Contracts
                                .SelectMany(x => x?.ContractAllocations)
                                .SingleOrDefault(y => y.FundingStreamPeriodCode.CaseInsensitiveEquals(fundingStreamPeriodCode))?.ContractAllocationNumber;

                            // populate the contract data fields in the appsMonthlyPaymentModel payment.
                            if (contractAllocationNumber != null)
                            {
                                appsMonthlyPaymentModel.FcsContractContractAllocationContractAllocationNumber =
                                    contractAllocationNumber;
                            }
                        }

                        //--------------------------------------------------------------------------------------------------
                        // process the learner fields
                        //--------------------------------------------------------------------------------------------------
                        if (_appsMonthlyPaymentIlrInfo?.Learners != null)
                        {
                            var ilrLearner = _appsMonthlyPaymentIlrInfo?.Learners?
                                .Where(x => x.LearnRefNumber.CaseInsensitiveEquals(appsMonthlyPaymentModel?.PaymentLearnerReferenceNumber))
                                .SingleOrDefault();

                            // populate the Learner data fields in the appsMonthlyPaymentModel payment.
                            if (ilrLearner != null)
                            {
                                appsMonthlyPaymentModel.LearnerCampusIdentifier = ilrLearner?.CampId;

                                //--------------------------------------------------------------------------------------------------------
                                // process the Learner Provider Specified Learner Monitoring fields in the appsMonthlyPaymentModel payment
                                //--------------------------------------------------------------------------------------------------------
                                if (ilrLearner?.ProviderSpecLearnerMonitorings != null)
                                {
                                    var ilrProviderSpecifiedLearnerMonitoringInfoList = ilrLearner?.ProviderSpecLearnerMonitorings
                                        ?.Where(pslm => pslm?.Ukprn == appsMonthlyPaymentModel?.Ukprn && pslm.LearnRefNumber.CaseInsensitiveEquals(appsMonthlyPaymentModel?.PaymentLearnerReferenceNumber))
                                        .ToList();

                                    if (ilrProviderSpecifiedLearnerMonitoringInfoList != null)
                                    {
                                        // populate the Provider Specified Learner Monitoring fields in the appsMonthlyPaymentModel payment.
                                        appsMonthlyPaymentModel.ProviderSpecifiedLearnerMonitoringA = ilrProviderSpecifiedLearnerMonitoringInfoList?.SingleOrDefault(x => (x?.ProvSpecLearnMonOccur).CaseInsensitiveEquals("A"))?.ProvSpecLearnMon;
                                        appsMonthlyPaymentModel.ProviderSpecifiedLearnerMonitoringB = ilrProviderSpecifiedLearnerMonitoringInfoList?.SingleOrDefault(x => (x?.ProvSpecLearnMonOccur).CaseInsensitiveEquals("B"))?.ProvSpecLearnMon;
                                    }
                                }

                                // Note: The Learner Employment Status fields are processed after the Learning Delivery data due
                                // to a dependency on the LearningDelivery.LearningStart date

                                //--------------------------------------------------------------------------------------------------
                                // process the learning delivery fields
                                //--------------------------------------------------------------------------------------------------
                                // Note: This code has a dependency on the Payment.EarningEvent.LearningAimSequenceNumber which should have been populated prior to reaching this point in the code.
                                AppsMonthlyPaymentLearningDeliveryModel learningDeliveryModel = null;

                                if (appsMonthlyPaymentModel?.PaymentEarningEventAimSeqNumber != null)
                                {
                                    learningDeliveryModel = ilrLearner?.LearningDeliveries?
                                        .Where(ld => ld?.Ukprn == appsMonthlyPaymentModel?.Ukprn &&
                                                     ld.LearnRefNumber.CaseInsensitiveEquals(appsMonthlyPaymentModel
                                                         ?.PaymentLearnerReferenceNumber) &&
                                                     ld?.AimSeqNumber == appsMonthlyPaymentModel
                                                         ?.PaymentEarningEventAimSeqNumber)
                                        .SingleOrDefault();
                                }

                                if (learningDeliveryModel != null)
                                {
                                    // populate the Learning Delivery fields in the appsMonthlyPaymentModel payment.
                                    appsMonthlyPaymentModel.LearningDeliveryOriginalLearningStartDate = learningDeliveryModel?.OrigLearnStartDate;
                                    appsMonthlyPaymentModel.LearningDeliveryLearningPlannedEndDate = learningDeliveryModel?.LearnPlanEndDate;
                                    appsMonthlyPaymentModel.LearningDeliveryCompletionStatus = learningDeliveryModel?.CompStatus;
                                    appsMonthlyPaymentModel.LearningDeliveryLearningActualEndDate = learningDeliveryModel?.LearnActEndDate;
                                    appsMonthlyPaymentModel.LearningDeliveryAchievementDate = learningDeliveryModel?.AchDate;
                                    appsMonthlyPaymentModel.LearningDeliveryOutcome = learningDeliveryModel?.Outcome;
                                    appsMonthlyPaymentModel.LearningDeliveryAimType = learningDeliveryModel?.AimType;
                                    appsMonthlyPaymentModel.LearningDeliverySoftwareSupplierAimIdentifier = learningDeliveryModel?.SwSupAimId;
                                    appsMonthlyPaymentModel.LearningDeliveryEndPointAssessmentOrganisation = learningDeliveryModel?.EpaOrgId;
                                    appsMonthlyPaymentModel.LearningDeliverySubContractedOrPartnershipUkprn = learningDeliveryModel?.PartnerUkprn;

                                    // The LD.AimSequenceNumber should match the PaymentEarningEventAimSeqNumber but may not if the PaymentEarningEventAimSeqNumber
                                    // was empty and we matched the Learning Delivery data by using the Prog fields. If they are different we assign the
                                    // LD AimSequenceNumber to the PaymentEarningEventAimSeqNumber so that we have consistent LD related data (FAMs and
                                    // ProvSpecDelMons) matching the LD data
                                    if (appsMonthlyPaymentModel?.PaymentEarningEventAimSeqNumber != learningDeliveryModel?.AimSeqNumber)
                                    {
                                        appsMonthlyPaymentModel.PaymentEarningEventAimSeqNumber = learningDeliveryModel?.AimSeqNumber;

                                        // TODO: log the difference between EarningsEvent AimSeqNumber and the Learning Delivery AimSequence
                                    }

                                    //-----------------------------------------------------------------------------------
                                    // populate the Learning Delivery FAM fields in the appsMonthlyPaymentModel payment.
                                    //-----------------------------------------------------------------------------------
                                    if (learningDeliveryModel?.LearningDeliveryFams != null)
                                    {
                                        var ilrLearningDeliveryFamInfoList = learningDeliveryModel?.LearningDeliveryFams
                                            ?
                                            .Where(fam => fam?.Ukprn == appsMonthlyPaymentModel?.Ukprn &&
                                                          fam.LearnRefNumber.CaseInsensitiveEquals(
                                                              appsMonthlyPaymentModel?.PaymentLearnerReferenceNumber) &&
                                                          fam?.AimSeqNumber == appsMonthlyPaymentModel
                                                              ?.PaymentEarningEventAimSeqNumber)
                                            .ToList();

                                        if (ilrLearningDeliveryFamInfoList != null)
                                        {
                                            appsMonthlyPaymentModel.LearningDeliveryFamTypeLearningDeliveryMonitoringA =
                                                ilrLearningDeliveryFamInfoList?
                                                    .SingleOrDefault(x =>
                                                        (x?.LearnDelFAMType).CaseInsensitiveEquals("LDM1"))
                                                    ?.LearnDelFAMCode;

                                            appsMonthlyPaymentModel.LearningDeliveryFamTypeLearningDeliveryMonitoringB =
                                                ilrLearningDeliveryFamInfoList?
                                                    .SingleOrDefault(x =>
                                                        (x?.LearnDelFAMType).CaseInsensitiveEquals("LDM2"))
                                                    ?.LearnDelFAMCode ?? string.Empty;

                                            appsMonthlyPaymentModel.LearningDeliveryFamTypeLearningDeliveryMonitoringC =
                                                ilrLearningDeliveryFamInfoList?
                                                    .SingleOrDefault(x =>
                                                        (x?.LearnDelFAMType).CaseInsensitiveEquals("LDM3"))
                                                    ?.LearnDelFAMCode ?? string.Empty;

                                            appsMonthlyPaymentModel.LearningDeliveryFamTypeLearningDeliveryMonitoringD =
                                                ilrLearningDeliveryFamInfoList?
                                                    .SingleOrDefault(x =>
                                                        (x?.LearnDelFAMType).CaseInsensitiveEquals("LDM4"))
                                                    ?.LearnDelFAMCode ?? string.Empty;

                                            appsMonthlyPaymentModel.LearningDeliveryFamTypeLearningDeliveryMonitoringE =
                                                ilrLearningDeliveryFamInfoList?
                                                    .SingleOrDefault(x =>
                                                        (x?.LearnDelFAMType).CaseInsensitiveEquals("LDM5"))
                                                    ?.LearnDelFAMCode ?? string.Empty;

                                            appsMonthlyPaymentModel.LearningDeliveryFamTypeLearningDeliveryMonitoringF =
                                                ilrLearningDeliveryFamInfoList?
                                                    .SingleOrDefault(x =>
                                                        (x?.LearnDelFAMType).CaseInsensitiveEquals("LDM6"))
                                                    ?.LearnDelFAMCode ?? string.Empty;
                                        }
                                    }

                                    //--------------------------------------------------------------------------------------------------
                                    // process the Provider Specified Delivery Monitoring fields in the appsMonthlyPaymentModel payment.
                                    //--------------------------------------------------------------------------------------------------
                                    if (learningDeliveryModel.ProviderSpecDeliveryMonitorings != null)
                                    {
                                        var ilrLearningDeliveryProviderSpecDeliveryMonitoringInfoList =
                                            learningDeliveryModel.ProviderSpecDeliveryMonitorings?
                                                .Where(psdm => psdm?.Ukprn == appsMonthlyPaymentModel?.Ukprn &&
                                                               psdm.LearnRefNumber.CaseInsensitiveEquals(
                                                                   appsMonthlyPaymentModel
                                                                       ?.PaymentLearnerReferenceNumber) &&
                                                               psdm?.AimSeqNumber == appsMonthlyPaymentModel
                                                                   ?.PaymentEarningEventAimSeqNumber)
                                                .ToList();

                                        if (ilrLearningDeliveryProviderSpecDeliveryMonitoringInfoList != null)
                                        {
                                            // populate the Provider Specified Delivery Monitoring fields in the appsMonthlyPaymentModel payment.
                                            appsMonthlyPaymentModel.ProviderSpecifiedDeliveryMonitoringA =
                                                ilrLearningDeliveryProviderSpecDeliveryMonitoringInfoList?
                                                    .SingleOrDefault(x =>
                                                        (x?.ProvSpecDelMonOccur).CaseInsensitiveEquals("A"))
                                                    ?.ProvSpecDelMon ?? string.Empty;

                                            appsMonthlyPaymentModel.ProviderSpecifiedDeliveryMonitoringB =
                                                ilrLearningDeliveryProviderSpecDeliveryMonitoringInfoList?
                                                    .SingleOrDefault(x =>
                                                        (x?.ProvSpecDelMonOccur).CaseInsensitiveEquals("B"))
                                                    ?.ProvSpecDelMon ?? string.Empty;

                                            appsMonthlyPaymentModel.ProviderSpecifiedDeliveryMonitoringC =
                                                ilrLearningDeliveryProviderSpecDeliveryMonitoringInfoList?
                                                    .SingleOrDefault(x =>
                                                        (x?.ProvSpecDelMonOccur).CaseInsensitiveEquals("C"))
                                                    ?.ProvSpecDelMon ?? string.Empty;

                                            appsMonthlyPaymentModel.ProviderSpecifiedDeliveryMonitoringD =
                                                ilrLearningDeliveryProviderSpecDeliveryMonitoringInfoList?
                                                    .SingleOrDefault(x =>
                                                        (x?.ProvSpecDelMonOccur).CaseInsensitiveEquals("D"))
                                                    ?.ProvSpecDelMon ?? string.Empty;
                                        }
                                    }

                                    //-------------------------------------------------------------------
                                    // process the rulebase fields in the appsMonthlyPaymentModel payment
                                    //-------------------------------------------------------------------
                                    if (_appsMonthlyPaymentRulebaseInfo?.AecApprenticeshipPriceEpisodeInfoList != null)
                                    {
                                        // process the AECPriceEpisode fields

                                        // get the price episode with the latest start date before the payment start date
                                        // NOTE: This code is dependent on the PaymentLearningStartDate being populated (done in the Learning Delivery population code)
                                        var ape = _appsMonthlyPaymentRulebaseInfo
                                            ?.AecApprenticeshipPriceEpisodeInfoList
                                            .Where(x => x?.Ukprn == appsMonthlyPaymentModel?.Ukprn &&
                                                        x.LearnRefNumber.CaseInsensitiveEquals(
                                                            appsMonthlyPaymentModel
                                                                ?.PaymentLearnerReferenceNumber) &&
                                                        x?.AimSequenceNumber == appsMonthlyPaymentModel
                                                            ?.PaymentEarningEventAimSeqNumber &&
                                                        x?.EpisodeStartDate <= appsMonthlyPaymentModel
                                                            ?.PaymentLearningStartDate)
                                            .OrderByDescending(x => x?.EpisodeStartDate)
                                            .FirstOrDefault();

                                        // populate the appsMonthlyPaymentModel fields
                                        if (ape != null)
                                        {
                                            appsMonthlyPaymentModel
                                                    .RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier =
                                                ape?.PriceEpisodeAgreeId;
                                            appsMonthlyPaymentModel
                                                    .RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate
                                                =
                                                ape?.PriceEpisodeActualEndDateIncEPA;
                                        }
                                    }

                                    //--------------------------------------------------------------------------------------
                                    // process the AECLearningDelivery fields
                                    //--------------------------------------------------------------------------------------
                                    if (_appsMonthlyPaymentRulebaseInfo?.AecLearningDeliveryInfoList != null)
                                    {
                                        // NOTE: This code is dependent on the Earning Event Aim Sequence number being populated (done in the Earning Event population code)
                                        var ald = _appsMonthlyPaymentRulebaseInfo.AecLearningDeliveryInfoList
                                            .SingleOrDefault(x => x?.Ukprn == appsMonthlyPaymentModel?.Ukprn &&
                                                                 x.LearnRefNumber.CaseInsensitiveEquals(
                                                                     appsMonthlyPaymentModel
                                                                         ?.PaymentLearnerReferenceNumber) &&
                                                                 x?.AimSequenceNumber == appsMonthlyPaymentModel
                                                                     ?.PaymentEarningEventAimSeqNumber &&
                                                                 x?.LearnAimRef == appsMonthlyPaymentModel
                                                                     ?.PaymentLearningAimReference);

                                        // populate the AECLearningDelivery fields
                                        if (ald != null)
                                        {
                                            appsMonthlyPaymentModel
                                                    .RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim
                                                = ald?.PlannedNumOnProgInstalm;
                                        }
                                    }

                                    //--------------------------------------------------------------------------------------
                                    // process the Learner Employment Status fields in the appsMonthlyPaymentModel payment
                                    // Note: This code is dependent on the LearningDelivery.LearnStartDate so this code must
                                    //       be done after processing the Learning Delivery data
                                    //--------------------------------------------------------------------------------------
                                    if (ilrLearner?.LearnerEmploymentStatus != null)
                                    {
                                        if (learningDeliveryModel.LearnStartDate != null)
                                        {
                                            var ilrLearnerEmploymentStatus = ilrLearner?.LearnerEmploymentStatus?
                                                .Where(les => les?.Ukprn == appsMonthlyPaymentModel.Ukprn &&
                                                              les.LearnRefNumber.CaseInsensitiveEquals(appsMonthlyPaymentModel?.PaymentLearnerReferenceNumber) &&
                                                              les?.DateEmpStatApp <= learningDeliveryModel?.LearnStartDate)
                                                .OrderByDescending(les => les?.DateEmpStatApp)
                                                .FirstOrDefault();

                                            if (ilrLearnerEmploymentStatus != null)
                                            {
                                                // populate the Provider Specified Learner Monitoring fields in the appsMonthlyPaymentModel payment.
                                                appsMonthlyPaymentModel.LearnerEmploymentStatusEmployerId = ilrLearnerEmploymentStatus?.EmpdId;
                                                appsMonthlyPaymentModel.LearnerEmploymentStatus = ilrLearnerEmploymentStatus?.EmpStat;
                                                appsMonthlyPaymentModel.LearnerEmploymentStatusDate = ilrLearnerEmploymentStatus?.DateEmpStatApp;
                                            }
                                        }
                                    }
                                } // if (LearningDeliveryInfo != null)
                            } // if (ilrLearner != null)
                        } // if (_appsMonthlyPaymentIlrInfo != null)

                        // Period totals
                        appsMonthlyPaymentModel.AugustTotalPayments = appsMonthlyPaymentModel?.AugustLevyPayments +
                                                                      appsMonthlyPaymentModel?.AugustCoInvestmentPayments +
                                                                      appsMonthlyPaymentModel?.AugustCoInvestmentDueFromEmployerPayments +
                                                                      appsMonthlyPaymentModel?.AugustEmployerAdditionalPayments +
                                                                      appsMonthlyPaymentModel?.AugustProviderAdditionalPayments +
                                                                      appsMonthlyPaymentModel?.AugustApprenticeAdditionalPayments +
                                                                      appsMonthlyPaymentModel?.AugustEnglishAndMathsPayments +
                                                                      appsMonthlyPaymentModel?.AugustLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.SeptemberTotalPayments = appsMonthlyPaymentModel?.SeptemberLevyPayments +
                                                                        appsMonthlyPaymentModel?.SeptemberCoInvestmentPayments +
                                                                        appsMonthlyPaymentModel?.SeptemberCoInvestmentDueFromEmployerPayments +
                                                                        appsMonthlyPaymentModel?.SeptemberEmployerAdditionalPayments +
                                                                        appsMonthlyPaymentModel?.SeptemberProviderAdditionalPayments +
                                                                        appsMonthlyPaymentModel?.SeptemberApprenticeAdditionalPayments +
                                                                        appsMonthlyPaymentModel?.SeptemberEnglishAndMathsPayments +
                                                                        appsMonthlyPaymentModel?.SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.OctoberTotalPayments = appsMonthlyPaymentModel?.OctoberLevyPayments +
                                                                       appsMonthlyPaymentModel?.OctoberCoInvestmentPayments +
                                                                       appsMonthlyPaymentModel?.OctoberCoInvestmentDueFromEmployerPayments +
                                                                       appsMonthlyPaymentModel?.OctoberEmployerAdditionalPayments +
                                                                       appsMonthlyPaymentModel?.OctoberProviderAdditionalPayments +
                                                                       appsMonthlyPaymentModel?.OctoberApprenticeAdditionalPayments +
                                                                       appsMonthlyPaymentModel?.OctoberEnglishAndMathsPayments +
                                                                       appsMonthlyPaymentModel?.OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.NovemberTotalPayments = appsMonthlyPaymentModel?.NovemberLevyPayments +
                                                                        appsMonthlyPaymentModel?.NovemberCoInvestmentPayments +
                                                                        appsMonthlyPaymentModel?.NovemberCoInvestmentDueFromEmployerPayments +
                                                                        appsMonthlyPaymentModel?.NovemberEmployerAdditionalPayments +
                                                                        appsMonthlyPaymentModel?.NovemberProviderAdditionalPayments +
                                                                        appsMonthlyPaymentModel?.NovemberApprenticeAdditionalPayments +
                                                                        appsMonthlyPaymentModel?.NovemberEnglishAndMathsPayments +
                                                                        appsMonthlyPaymentModel?.NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.DecemberTotalPayments = appsMonthlyPaymentModel?.DecemberLevyPayments +
                                                                        appsMonthlyPaymentModel?.DecemberCoInvestmentPayments +
                                                                        appsMonthlyPaymentModel?.DecemberCoInvestmentDueFromEmployerPayments +
                                                                        appsMonthlyPaymentModel?.DecemberEmployerAdditionalPayments +
                                                                        appsMonthlyPaymentModel?.DecemberProviderAdditionalPayments +
                                                                        appsMonthlyPaymentModel?.DecemberApprenticeAdditionalPayments +
                                                                        appsMonthlyPaymentModel?.DecemberEnglishAndMathsPayments +
                                                                        appsMonthlyPaymentModel?.DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.JanuaryTotalPayments = appsMonthlyPaymentModel?.JanuaryLevyPayments +
                                                                       appsMonthlyPaymentModel
                                                                           ?.JanuaryCoInvestmentPayments +
                                                                       appsMonthlyPaymentModel
                                                                           ?.JanuaryCoInvestmentDueFromEmployerPayments +
                                                                       appsMonthlyPaymentModel
                                                                           ?.JanuaryEmployerAdditionalPayments +
                                                                       appsMonthlyPaymentModel
                                                                           ?.JanuaryProviderAdditionalPayments +
                                                                       appsMonthlyPaymentModel
                                                                           ?.JanuaryApprenticeAdditionalPayments +
                                                                       appsMonthlyPaymentModel
                                                                           ?.JanuaryEnglishAndMathsPayments +
                                                                       appsMonthlyPaymentModel
                                                                           ?.JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.FebruaryTotalPayments =
                            appsMonthlyPaymentModel?.FebruaryLevyPayments +
                            appsMonthlyPaymentModel?.FebruaryCoInvestmentPayments +
                            appsMonthlyPaymentModel?.FebruaryCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.FebruaryEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.FebruaryProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.FebruaryApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.FebruaryEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.MarchTotalPayments = appsMonthlyPaymentModel?.MarchLevyPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.MarchCoInvestmentPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.MarchCoInvestmentDueFromEmployerPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.MarchEmployerAdditionalPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.MarchProviderAdditionalPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.MarchApprenticeAdditionalPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.MarchEnglishAndMathsPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.MarchLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.AprilTotalPayments = appsMonthlyPaymentModel?.AprilLevyPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.AprilCoInvestmentPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.AprilCoInvestmentDueFromEmployerPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.AprilEmployerAdditionalPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.AprilProviderAdditionalPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.AprilApprenticeAdditionalPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.AprilEnglishAndMathsPayments +
                                                                     appsMonthlyPaymentModel
                                                                         ?.AprilLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.MayTotalPayments = appsMonthlyPaymentModel?.MayLevyPayments +
                                                                   appsMonthlyPaymentModel?.MayCoInvestmentPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.MayCoInvestmentDueFromEmployerPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.MayEmployerAdditionalPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.MayProviderAdditionalPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.MayApprenticeAdditionalPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.MayEnglishAndMathsPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.MayLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.JuneTotalPayments = appsMonthlyPaymentModel?.JuneLevyPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JuneCoInvestmentPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JuneCoInvestmentDueFromEmployerPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JuneEmployerAdditionalPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JuneProviderAdditionalPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JuneApprenticeAdditionalPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JuneEnglishAndMathsPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JuneLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.JulyTotalPayments = appsMonthlyPaymentModel?.JulyLevyPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JulyCoInvestmentPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JulyCoInvestmentDueFromEmployerPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JulyEmployerAdditionalPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JulyProviderAdditionalPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JulyApprenticeAdditionalPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JulyEnglishAndMathsPayments +
                                                                    appsMonthlyPaymentModel
                                                                        ?.JulyLearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.R13TotalPayments = appsMonthlyPaymentModel?.R13LevyPayments +
                                                                   appsMonthlyPaymentModel?.R13CoInvestmentPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R13CoInvestmentDueFromEmployerPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R13EmployerAdditionalPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R13ProviderAdditionalPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R13ApprenticeAdditionalPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R13EnglishAndMathsPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R13LearningSupportDisadvantageAndFrameworkUpliftPayments;

                        appsMonthlyPaymentModel.R14TotalPayments = appsMonthlyPaymentModel?.R14LevyPayments +
                                                                   appsMonthlyPaymentModel?.R14CoInvestmentPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R14CoInvestmentDueFromEmployerPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R14EmployerAdditionalPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R14ProviderAdditionalPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R14ApprenticeAdditionalPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R14EnglishAndMathsPayments +
                                                                   appsMonthlyPaymentModel
                                                                       ?.R14LearningSupportDisadvantageAndFrameworkUpliftPayments;

                        // Academic year totals

                        // Total Levy payments
                        appsMonthlyPaymentModel.TotalLevyPayments = appsMonthlyPaymentModel?.AugustLevyPayments +
                                                                    appsMonthlyPaymentModel?.SeptemberLevyPayments +
                                                                    appsMonthlyPaymentModel?.OctoberLevyPayments +
                                                                    appsMonthlyPaymentModel?.NovemberLevyPayments +
                                                                    appsMonthlyPaymentModel?.DecemberLevyPayments +
                                                                    appsMonthlyPaymentModel?.JanuaryLevyPayments +
                                                                    appsMonthlyPaymentModel?.FebruaryLevyPayments +
                                                                    appsMonthlyPaymentModel?.MarchLevyPayments +
                                                                    appsMonthlyPaymentModel?.AprilLevyPayments +
                                                                    appsMonthlyPaymentModel?.MayLevyPayments +
                                                                    appsMonthlyPaymentModel?.JuneLevyPayments +
                                                                    appsMonthlyPaymentModel?.JulyLevyPayments +
                                                                    appsMonthlyPaymentModel?.R13LevyPayments +
                                                                    appsMonthlyPaymentModel?.R14LevyPayments;

                        // Total CoInvestment totals
                        appsMonthlyPaymentModel.TotalCoInvestmentPayments =
                            appsMonthlyPaymentModel?.AugustCoInvestmentPayments +
                            appsMonthlyPaymentModel?.SeptemberCoInvestmentPayments +
                            appsMonthlyPaymentModel?.OctoberCoInvestmentPayments +
                            appsMonthlyPaymentModel?.NovemberCoInvestmentPayments +
                            appsMonthlyPaymentModel?.DecemberCoInvestmentPayments +
                            appsMonthlyPaymentModel?.JanuaryCoInvestmentPayments +
                            appsMonthlyPaymentModel?.FebruaryCoInvestmentPayments +
                            appsMonthlyPaymentModel?.MarchCoInvestmentPayments +
                            appsMonthlyPaymentModel?.AprilCoInvestmentPayments +
                            appsMonthlyPaymentModel?.MayCoInvestmentPayments +
                            appsMonthlyPaymentModel?.JuneCoInvestmentPayments +
                            appsMonthlyPaymentModel?.JulyCoInvestmentPayments +
                            appsMonthlyPaymentModel?.R13CoInvestmentPayments +
                            appsMonthlyPaymentModel?.R14CoInvestmentPayments;

                        // Total CoInvestment due from employer
                        appsMonthlyPaymentModel.TotalCoInvestmentDueFromEmployerPayments =
                            appsMonthlyPaymentModel?.AugustCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.SeptemberCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.OctoberCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.NovemberCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.DecemberCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.JanuaryCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.FebruaryCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.MarchCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.AprilCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.MayCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.JuneCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.JulyCoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.R13CoInvestmentDueFromEmployerPayments +
                            appsMonthlyPaymentModel?.R14CoInvestmentDueFromEmployerPayments;

                        // Total Employer Additional payments
                        appsMonthlyPaymentModel.TotalEmployerAdditionalPayments =
                            appsMonthlyPaymentModel?.AugustEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.SeptemberEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.OctoberEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.NovemberEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.DecemberEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.JanuaryEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.FebruaryEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.MarchEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.AprilEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.MayEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.JuneEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.JulyEmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.R13EmployerAdditionalPayments +
                            appsMonthlyPaymentModel?.R14EmployerAdditionalPayments;

                        // Total Provider Additional payments
                        appsMonthlyPaymentModel.TotalProviderAdditionalPayments =
                            appsMonthlyPaymentModel?.AugustProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.SeptemberProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.OctoberProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.NovemberProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.DecemberProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.JanuaryProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.FebruaryProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.MarchProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.AprilProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.MayProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.JuneProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.JulyProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.R13ProviderAdditionalPayments +
                            appsMonthlyPaymentModel?.R14ProviderAdditionalPayments;

                        // Total Apprentice Additional payments
                        appsMonthlyPaymentModel.TotalApprenticeAdditionalPayments =
                            appsMonthlyPaymentModel?.AugustApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.SeptemberApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.OctoberApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.NovemberApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.DecemberApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.JanuaryApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.FebruaryApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.MarchApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.AprilApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.MayApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.JuneApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.JulyApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.R13ApprenticeAdditionalPayments +
                            appsMonthlyPaymentModel?.R14ApprenticeAdditionalPayments;

                        // Total English and Maths payments
                        appsMonthlyPaymentModel.TotalEnglishAndMathsPayments =
                            appsMonthlyPaymentModel?.AugustEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.SeptemberEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.OctoberEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.NovemberEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.DecemberEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.JanuaryEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.FebruaryEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.MarchEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.AprilEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.MayEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.JuneEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.JulyEnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.R13EnglishAndMathsPayments +
                            appsMonthlyPaymentModel?.R14EnglishAndMathsPayments;

                        // Total Learning Support, Disadvantage and Framework Uplifts
                        appsMonthlyPaymentModel.TotalLearningSupportDisadvantageAndFrameworkUpliftPayments =
                            appsMonthlyPaymentModel?.AugustLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.MarchLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.AprilLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.MayLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.JuneLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.JulyLearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.R13LearningSupportDisadvantageAndFrameworkUpliftPayments +
                            appsMonthlyPaymentModel?.R14LearningSupportDisadvantageAndFrameworkUpliftPayments;

                        // Total payments
                        appsMonthlyPaymentModel.TotalPayments = appsMonthlyPaymentModel.TotalPayments =
                            appsMonthlyPaymentModel?.AugustTotalPayments +
                            appsMonthlyPaymentModel?.SeptemberTotalPayments +
                            appsMonthlyPaymentModel?.OctoberTotalPayments +
                            appsMonthlyPaymentModel?.NovemberTotalPayments +
                            appsMonthlyPaymentModel?.DecemberTotalPayments +
                            appsMonthlyPaymentModel?.JanuaryTotalPayments +
                            appsMonthlyPaymentModel?.FebruaryTotalPayments +
                            appsMonthlyPaymentModel?.MarchTotalPayments +
                            appsMonthlyPaymentModel?.AprilTotalPayments +
                            appsMonthlyPaymentModel?.MayTotalPayments +
                            appsMonthlyPaymentModel?.JuneTotalPayments +
                            appsMonthlyPaymentModel?.JulyTotalPayments +
                            appsMonthlyPaymentModel?.R13TotalPayments +
                            appsMonthlyPaymentModel?.R14TotalPayments;
                    } // foreach (var appsMonthlyPaymentModel in appsMonthlyPaymentModelList)
                }
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                var y = ex;
                throw;
                //_logger.LogError("Failed to get Rulebase data", ex);
            }

            return appsMonthlyPaymentModelList;
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
                   payment.LearningAimReference.CaseInsensitiveEquals(ZPROG001) &&
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
                          payment.LearningAimReference.CaseInsensitiveEquals(ZPROG001) &&
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
                          payment.LearningAimReference.CaseInsensitiveEquals(ZPROG001) &&
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
                          payment.LearningAimReference.CaseInsensitiveEquals(ZPROG001) &&
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
                          payment.LearningAimReference.CaseInsensitiveEquals(ZPROG001) &&
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
                   payment.LearningAimReference.CaseInsensitiveEquals(ZPROG001) &&
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
                          !payment.LearningAimReference.CaseInsensitiveEquals(ZPROG001) &&
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
                           ((payment.LearningAimReference.CaseInsensitiveEquals(ZPROG001) &&
                           _transactionTypesLearningSupportPayments.Contains(payment.TransactionType)) ||
                           (!payment.LearningAimReference.CaseInsensitiveEquals(ZPROG001) && payment.TransactionType == 15));

            if (result)
            {
                var t = result;
            }

            return result;
        }
    }
}
