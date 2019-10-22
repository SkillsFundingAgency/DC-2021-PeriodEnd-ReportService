using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd
{
    public class AppsMonthlyPaymentModel
    {
        public int? Ukprn { get; set; }

        public int? ReturnPeriod { get; set; }

        public string PaymentLearnerReferenceNumber { get; set; }

        public long? PaymentUniqueLearnerNumber { get; set; }

        public string LearnerCampusIdentifier { get; set; }

        public string ProviderSpecifiedLearnerMonitoringA { get; set; }

        public string ProviderSpecifiedLearnerMonitoringB { get; set; }

        // public Guid? PaymentEarningEventId { get; set; }

        public byte? PaymentEarningEventAimSeqNumber { get; set; }

        public string PaymentLearningAimReference { get; set; }

        public string LarsLearningDeliveryLearningAimTitle { get; set; }

        public DateTime? LearningDeliveryOriginalLearningStartDate { get; set; }

        public DateTime? PaymentLearningStartDate { get; set; }

        public DateTime? LearningDeliveryLearningPlannedEndDate { get; set; }

        public int? LearningDeliveryCompletionStatus { get; set; }

        public DateTime? LearningDeliveryLearningActualEndDate { get; set; }

        public DateTime? LearningDeliveryAchievementDate { get; set; }

        public int? LearningDeliveryOutcome { get; set; }

        public int? PaymentProgrammeType { get; set; }

        public int? PaymentStandardCode { get; set; }

        public int? PaymentFrameworkCode { get; set; }

        public int? PaymentPathwayCode { get; set; }

        public int? LearningDeliveryAimType { get; set; }

        public string LearningDeliverySoftwareSupplierAimIdentifier { get; set; }

        public string LearningDeliveryFamTypeLearningDeliveryMonitoringA { get; set; }

        public string LearningDeliveryFamTypeLearningDeliveryMonitoringB { get; set; }

        public string LearningDeliveryFamTypeLearningDeliveryMonitoringC { get; set; }

        public string LearningDeliveryFamTypeLearningDeliveryMonitoringD { get; set; }

        public string LearningDeliveryFamTypeLearningDeliveryMonitoringE { get; set; }

        public string LearningDeliveryFamTypeLearningDeliveryMonitoringF { get; set; }

        public string ProviderSpecifiedDeliveryMonitoringA { get; set; }

        public string ProviderSpecifiedDeliveryMonitoringB { get; set; }

        public string ProviderSpecifiedDeliveryMonitoringC { get; set; }

        public string ProviderSpecifiedDeliveryMonitoringD { get; set; }

        public string LearningDeliveryEndPointAssessmentOrganisation { get; set; }

        public int? RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim { get; set; }

        public int? LearningDeliverySubContractedOrPartnershipUkprn { get; set; }

        public string PaymentPriceEpisodeIdentifier { get; set; }

        public string PaymentPriceEpisodeStartDate { get; set; }

        public DateTime? RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate { get; set; }

        public string FcsContractContractAllocationContractAllocationNumber { get; set; }

        public string PaymentFundingLineType { get; set; }

        public byte? PaymentApprenticeshipContractType { get; set; }

        public int? LearnerEmploymentStatusEmployerId { get; set; }

        public string RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier { get; set; }

        public int? LearnerEmploymentStatus { get; set; }

        public DateTime? LearnerEmploymentStatusDate { get; set; }

        // Period payments - August (R01)
        public decimal? AugustLevyPayments { get; set; }

        public decimal? AugustCoInvestmentPayments { get; set; }

        public decimal? AugustCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? AugustEmployerAdditionalPayments { get; set; }

        public decimal? AugustProviderAdditionalPayments { get; set; }

        public decimal? AugustApprenticeAdditionalPayments { get; set; }

        public decimal? AugustEnglishAndMathsPayments { get; set; }

        public decimal? AugustLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? AugustTotalPayments { get; set; }

        // Period payments - September (R02)
        public decimal? SeptemberLevyPayments { get; set; }

        public decimal? SeptemberCoInvestmentPayments { get; set; }

        public decimal? SeptemberCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? SeptemberEmployerAdditionalPayments { get; set; }

        public decimal? SeptemberProviderAdditionalPayments { get; set; }

        public decimal? SeptemberApprenticeAdditionalPayments { get; set; }

        public decimal? SeptemberEnglishAndMathsPayments { get; set; }

        public decimal? SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? SeptemberTotalPayments { get; set; }

        // Period payments - October (R03)
        public decimal? OctoberLevyPayments { get; set; }

        public decimal? OctoberCoInvestmentPayments { get; set; }

        public decimal? OctoberCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? OctoberEmployerAdditionalPayments { get; set; }

        public decimal? OctoberProviderAdditionalPayments { get; set; }

        public decimal? OctoberApprenticeAdditionalPayments { get; set; }

        public decimal? OctoberEnglishAndMathsPayments { get; set; }

        public decimal? OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? OctoberTotalPayments { get; set; }

        // Period payments - November (R04)
        public decimal? NovemberLevyPayments { get; set; }

        public decimal? NovemberCoInvestmentPayments { get; set; }

        public decimal? NovemberCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? NovemberEmployerAdditionalPayments { get; set; }

        public decimal? NovemberProviderAdditionalPayments { get; set; }

        public decimal? NovemberApprenticeAdditionalPayments { get; set; }

        public decimal? NovemberEnglishAndMathsPayments { get; set; }

        public decimal? NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? NovemberTotalPayments { get; set; }

        // Period payments - December (R05)
        public decimal? DecemberLevyPayments { get; set; }

        public decimal? DecemberCoInvestmentPayments { get; set; }

        public decimal? DecemberCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? DecemberEmployerAdditionalPayments { get; set; }

        public decimal? DecemberProviderAdditionalPayments { get; set; }

        public decimal? DecemberApprenticeAdditionalPayments { get; set; }

        public decimal? DecemberEnglishAndMathsPayments { get; set; }

        public decimal? DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? DecemberTotalPayments { get; set; }

        // Period payments - January (R06)
        public decimal? JanuaryLevyPayments { get; set; }

        public decimal? JanuaryCoInvestmentPayments { get; set; }

        public decimal? JanuaryCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? JanuaryEmployerAdditionalPayments { get; set; }

        public decimal? JanuaryProviderAdditionalPayments { get; set; }

        public decimal? JanuaryApprenticeAdditionalPayments { get; set; }

        public decimal? JanuaryEnglishAndMathsPayments { get; set; }

        public decimal? JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? JanuaryTotalPayments { get; set; }

        // Period payments - February (R07)
        public decimal? FebruaryLevyPayments { get; set; }

        public decimal? FebruaryCoInvestmentPayments { get; set; }

        public decimal? FebruaryCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? FebruaryEmployerAdditionalPayments { get; set; }

        public decimal? FebruaryProviderAdditionalPayments { get; set; }

        public decimal? FebruaryApprenticeAdditionalPayments { get; set; }

        public decimal? FebruaryEnglishAndMathsPayments { get; set; }

        public decimal? FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? FebruaryTotalPayments { get; set; }

        // Period payments - March (R08)
        public decimal? MarchLevyPayments { get; set; }

        public decimal? MarchCoInvestmentPayments { get; set; }

        public decimal? MarchCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? MarchEmployerAdditionalPayments { get; set; }

        public decimal? MarchProviderAdditionalPayments { get; set; }

        public decimal? MarchApprenticeAdditionalPayments { get; set; }

        public decimal? MarchEnglishAndMathsPayments { get; set; }

        public decimal? MarchLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? MarchTotalPayments { get; set; }

        // Period payments - April (R09)
        public decimal? AprilLevyPayments { get; set; }

        public decimal? AprilCoInvestmentPayments { get; set; }

        public decimal? AprilCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? AprilEmployerAdditionalPayments { get; set; }

        public decimal? AprilProviderAdditionalPayments { get; set; }

        public decimal? AprilApprenticeAdditionalPayments { get; set; }

        public decimal? AprilEnglishAndMathsPayments { get; set; }

        public decimal? AprilLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? AprilTotalPayments { get; set; }

        // Period payments - May (R10)
        public decimal? MayLevyPayments { get; set; }

        public decimal? MayCoInvestmentPayments { get; set; }

        public decimal? MayCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? MayEmployerAdditionalPayments { get; set; }

        public decimal? MayProviderAdditionalPayments { get; set; }

        public decimal? MayApprenticeAdditionalPayments { get; set; }

        public decimal? MayEnglishAndMathsPayments { get; set; }

        public decimal? MayLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? MayTotalPayments { get; set; }

        // Period payments - June (R11)
        public decimal? JuneLevyPayments { get; set; }

        public decimal? JuneCoInvestmentPayments { get; set; }

        public decimal? JuneCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? JuneEmployerAdditionalPayments { get; set; }

        public decimal? JuneProviderAdditionalPayments { get; set; }

        public decimal? JuneApprenticeAdditionalPayments { get; set; }

        public decimal? JuneEnglishAndMathsPayments { get; set; }

        public decimal? JuneLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? JuneTotalPayments { get; set; }

        // Period payments - July (R12)
        public decimal? JulyLevyPayments { get; set; }

        public decimal? JulyCoInvestmentPayments { get; set; }

        public decimal? JulyCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? JulyEmployerAdditionalPayments { get; set; }

        public decimal? JulyProviderAdditionalPayments { get; set; }

        public decimal? JulyApprenticeAdditionalPayments { get; set; }

        public decimal? JulyEnglishAndMathsPayments { get; set; }

        public decimal? JulyLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? JulyTotalPayments { get; set; }

        // Period payments - R13
        public decimal? R13LevyPayments { get; set; }

        public decimal? R13CoInvestmentPayments { get; set; }

        public decimal? R13CoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? R13EmployerAdditionalPayments { get; set; }

        public decimal? R13ProviderAdditionalPayments { get; set; }

        public decimal? R13ApprenticeAdditionalPayments { get; set; }

        public decimal? R13EnglishAndMathsPayments { get; set; }

        public decimal? R13LearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? R13TotalPayments { get; set; }

        // Period payments - R14
        public decimal? R14LevyPayments { get; set; }

        public decimal? R14CoInvestmentPayments { get; set; }

        public decimal? R14CoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? R14EmployerAdditionalPayments { get; set; }

        public decimal? R14ProviderAdditionalPayments { get; set; }

        public decimal? R14ApprenticeAdditionalPayments { get; set; }

        public decimal? R14EnglishAndMathsPayments { get; set; }

        public decimal? R14LearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? R14TotalPayments { get; set; }

        // Payments totals
        public decimal? TotalLevyPayments { get; set; }

        public decimal? TotalCoInvestmentPayments { get; set; }

        public decimal? TotalCoInvestmentDueFromEmployerPayments { get; set; }

        public decimal? TotalEmployerAdditionalPayments { get; set; }

        public decimal? TotalProviderAdditionalPayments { get; set; }

        public decimal? TotalApprenticeAdditionalPayments { get; set; }

        public decimal? TotalEnglishAndMathsPayments { get; set; }

        public decimal? TotalLearningSupportDisadvantageAndFrameworkUpliftPayments { get; set; }

        public decimal? TotalPayments { get; set; }

        public string OfficialSensitive { get; set; }

        //public decimal?[] TotalMonthlyPayments { get; set; }
    }
}