﻿using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.DataPersist.Model
{
    public partial class McaGlaDevolvedOccupancyReportV2
    {
        public string Year { get; set; }
        public int Return { get; set; }
        public int AcMnth { get; set; }
        public int Ukprn { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public long? UniqueLearnerNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int Ethnicity { get; set; }
        public string Sex { get; set; }
        public int? LlddandHealthProblem { get; set; }
        public int? PriorAttainment { get; set; }
        public string PostcodePriorToEnrolment { get; set; }
        public int? PreMergerUkprn { get; set; }
        public string CampusIdentifier { get; set; }
        public string ProviderSpecifiedLearnerMonitoringA { get; set; }
        public string ProviderSpecifiedLearnerMonitoringB { get; set; }
        public int AimSeqNumber { get; set; }
        public string LearningAimReference { get; set; }
        public string LearningAimTitle { get; set; }
        public string SoftwareSupplierAimIdentifier { get; set; }
        public decimal? ApplicableFundingRateFromEsolhours { get; set; }
        public decimal? ApplicableFundingRate { get; set; }
        public string ApplicableProgrammeWeighting { get; set; }
        public decimal? AimValue { get; set; }
        public string NotionalNvqlevel { get; set; }
        public string EntitlementCategoryLevel2or3 { get; set; }
        public decimal? Tier2SectorSubjectArea { get; set; }
        public int? FundingModel { get; set; }
        public int? FundingAdjustmentForPriorLearning { get; set; }
        public int? OtherFundingAdjment { get; set; }
        public DateTime? OriginalLearningStartDate { get; set; }
        public DateTime? LearningStartDate { get; set; }
        public DateTime? LearningPlannedEndDate { get; set; }
        public int? CompletionStatus { get; set; }
        public DateTime? LearningActualEndDate { get; set; }
        public int? Outcome { get; set; }
        public int? AdditionalDeliveryHours { get; set; }
        public string LearningStartDatePostcode { get; set; }
        public string ApplicableAreaFromSourceOfFunding { get; set; }
        public string LdfamtypeSourceOfFunding { get; set; }
        public string LdfamtypeFullOrCoFundingIndicator { get; set; }
        public string LdfamtypeLearningSupportFundingHighestApplicable { get; set; }
        public DateTime? LdfamtypeLsfdateAppliesFromEarliest { get; set; }
        public DateTime? LdfamtypeLsfdateAppliesToLatest { get; set; }
        public string LdfamtypeLdma { get; set; }
        public string LdfamtypeLdmb { get; set; }
        public string LdfamtypeLdmc { get; set; }
        public string LdfamtypeLdmd { get; set; }
        public string LdfamtypeLdme { get; set; }
        public string LdfamtypeLdmf { get; set; }
        public string LdfamtypeDama { get; set; }
        public string LdfamtypeDamb { get; set; }
        public string LdfamtypeDamc { get; set; }
        public string LdfamtypeDamd { get; set; }
        public string LdfamtypeRestartIndicator { get; set; }
        public string LdfamtypeCommunityLearningProvisionType { get; set; }
        public string ProviderSpecifiedDeliveryMonitoringA { get; set; }
        public string ProviderSpecifiedDeliveryMonitoringB { get; set; }
        public string ProviderSpecifiedDeliveryMonitoringC { get; set; }
        public string ProviderSpecifiedDeliveryMonitoringD { get; set; }
        public int? LearnerEmploymentStatus { get; set; }
        public int? EsmtypeBenefitStatusIndicator { get; set; }
        public string FundingLineType { get; set; }
        public int? PlannedNumberOfOnProgrammeInstalments { get; set; }
        public decimal? AchievementElement { get; set; }
        public decimal? AchievementPercentage { get; set; }
        public decimal? NonPublicFundedContribution { get; set; }
        public decimal? CappingFactor { get; set; }
        public int? PartnerUkprn { get; set; }
        public string DeliveryLocationPostcode { get; set; }
        public decimal? AreaUplift { get; set; }
        public decimal? DisadvantageUplift { get; set; }
        public DateTime? DateUsedForUpliftsAndOtherLookups { get; set; }
        public decimal? AugustOnProgrammeEarnedCash { get; set; }
        public decimal? AugustBalancingPaymentEarnedCash { get; set; }
        public decimal? AugustAimAchievementEarnedCash { get; set; }
        public decimal? AugustJobOutcomeEarnedCash { get; set; }
        public decimal? AugustLearningSupportEarnedCash { get; set; }
        public decimal? SeptemberOnProgrammeEarnedCash { get; set; }
        public decimal? SeptemberBalancingPaymentEarnedcash { get; set; }
        public decimal? SeptemberAimAchievementEarnedCash { get; set; }
        public decimal? SeptemberJobOutcomeEarnedCash { get; set; }
        public decimal? SeptemberLearningSupportEarnedCash { get; set; }
        public decimal? OctoberOnProgrammeEarnedCash { get; set; }
        public decimal? OctoberBalancingPaymentEarnedcash { get; set; }
        public decimal? OctoberAimAchievementEarnedCash { get; set; }
        public decimal? OctoberJobOutcomeEarnedCash { get; set; }
        public decimal? OctoberLearningSupportEarnedCash { get; set; }
        public decimal? NovemberOnProgrammeEarnedCash { get; set; }
        public decimal? NovemberBalancingPaymentEarnedcash { get; set; }
        public decimal? NovemberAimAchievementEarnedCash { get; set; }
        public decimal? NovemberJobOutcomeEarnedCash { get; set; }
        public decimal? NovemberLearningSupportEarnedCash { get; set; }
        public decimal? DecemberOnProgrammeEarnedCash { get; set; }
        public decimal? DecemberBalancingPaymentEarnedcash { get; set; }
        public decimal? DecemberAimAchievementEarnedCash { get; set; }
        public decimal? DecemberJobOutcomeEarnedCash { get; set; }
        public decimal? DecemberLearningSupportEarnedCash { get; set; }
        public decimal? JanuaryOnProgrammeEarnedCash { get; set; }
        public decimal? JanuaryBalancingPaymentEarnedcash { get; set; }
        public decimal? JanuaryAimAchievementEarnedCash { get; set; }
        public decimal? JanuaryJobOutcomeEarnedCash { get; set; }
        public decimal? JanuaryLearningSupportEarnedCash { get; set; }
        public decimal? FebruaryOnProgrammeEarnedCash { get; set; }
        public decimal? FebruaryBalancingPaymentEarnedcash { get; set; }
        public decimal? FebruaryAimAchievementEarnedCash { get; set; }
        public decimal? FebruaryJobOutcomeEarnedCash { get; set; }
        public decimal? FebruaryLearningSupportEarnedCash { get; set; }
        public decimal? MarchOnProgrammeEarnedCash { get; set; }
        public decimal? MarchBalancingPaymentEarnedcash { get; set; }
        public decimal? MarchAimAchievementEarnedCash { get; set; }
        public decimal? MarchJobOutcomeEarnedCash { get; set; }
        public decimal? MarchLearningSupportEarnedCash { get; set; }
        public decimal? AprilOnProgrammeEarnedCash { get; set; }
        public decimal? AprilBalancingPaymentEarnedcash { get; set; }
        public decimal? AprilAimAchievementEarnedCash { get; set; }
        public decimal? AprilJobOutcomeEarnedCash { get; set; }
        public decimal? AprilLearningSupportEarnedCash { get; set; }
        public decimal? MayOnProgrammeEarnedCash { get; set; }
        public decimal? MayBalancingPaymentEarnedcash { get; set; }
        public decimal? MayAimAchievementEarnedCash { get; set; }
        public decimal? MayJobOutcomeEarnedCash { get; set; }
        public decimal? MayLearningSupportEarnedCash { get; set; }
        public decimal? JuneOnProgrammeEarnedCash { get; set; }
        public decimal? JuneBalancingPaymentEarnedcash { get; set; }
        public decimal? JuneAimAchievementEarnedCash { get; set; }
        public decimal? JuneJobOutcomeEarnedCash { get; set; }
        public decimal? JuneLearningSupportEarnedCash { get; set; }
        public decimal? JulyOnProgrammeEarnedCash { get; set; }
        public decimal? JulyBalancingPaymentEarnedcash { get; set; }
        public decimal? JulyAimAchievementEarnedCash { get; set; }
        public decimal? JulyJobOutcomeEarnedCash { get; set; }
        public decimal? JulyLearningSupportEarnedCash { get; set; }
        public decimal? TotalOnProgrammeEarnedCash { get; set; }
        public decimal? TotalBalancingPaymentEarnedCash { get; set; }
        public decimal? TotalAimAchievementEarnedCash { get; set; }
        public decimal? TotalJobOutcomeEarnedCash { get; set; }
        public decimal? TotalLearningSupportEarnedCash { get; set; }
        public decimal? TotalEarnedCash { get; set; }
        public string Officialsensitive { get; set; }
        public DateTime? CreatedDateTime { get; set; }
    }
}
