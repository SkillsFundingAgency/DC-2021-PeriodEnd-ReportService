using System.Globalization;
using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Mapper
{
    public class AppsMonthlyPaymentMapper : ClassMap<AppsMonthlyPaymentModel>
    {
        public AppsMonthlyPaymentMapper()
        {
            int i = 0;
            Map(m => m.PaymentLearnerReferenceNumber).Index(i++).Name("Learner reference number");
            Map(m => m.PaymentUniqueLearnerNumber).Index(i++).Name("Unique learner number");

            Map(m => m.LearnerCampusIdentifier).Index(i++).Name("Campus identifier");

            Map(m => m.ProviderSpecifiedLearnerMonitoringA).Index(i++).Name("Provider specified learner monitoring (A)");
            Map(m => m.ProviderSpecifiedLearnerMonitoringB).Index(i++).Name("Provider specified learner monitoring (B)");

            Map(m => m.PaymentEarningEventAimSeqNumber).Index(i++).Name("Aim sequence number");
            Map(m => m.PaymentLearningAimReference).Index(i++).Name("Learning aim reference");

            Map(m => m.LarsLearningDeliveryLearningAimTitle).Index(i++).Name("Learning aim title");

            Map(m => m.LearningDeliveryOriginalLearningStartDate).Index(i++).Name("Original learning start date");

            Map(m => m.PaymentLearningStartDate).Index(i++).Name("Learning start date");

            Map(m => m.LearningDeliveryLearningPlannedEndData).Index(i++).Name("Learning planned end date");
            Map(m => m.LearningDeliveryCompletionStatus).Index(i++).Name("Completion status");
            Map(m => m.LearningDeliveryLearningActualEndDate).Index(i++).Name("Learning actual end date");
            Map(m => m.LearningDeliveryAchievementDate).Index(i++).Name("Achievement date");
            Map(m => m.LearningDeliveryOutcome).Index(i++).Name("Outcome");

            Map(m => m.PaymentProgrammeType).Index(i++).Name("Programme type");
            Map(m => m.PaymentStandardCode).Index(i++).Name("Standard code");
            Map(m => m.PaymentFrameworkCode).Index(i++).Name("Framework code");
            Map(m => m.PaymentPathwayCode).Index(i++).Name("Apprenticeship pathway");

            Map(m => m.LearningDeliveryAimType).Index(i++).Name("Aim type");
            Map(m => m.LearningDeliverySoftwareSupplierAimIdentifier).Index(i++).Name("Software supplier aim identifier");

            Map(m => m.LearningDeliveryFamTypeLearningDeliveryMonitoringA).Index(i++).Name("Learning delivery funding and monitoring type – learning delivery monitoring (A)");
            Map(m => m.LearningDeliveryFamTypeLearningDeliveryMonitoringB).Index(i++).Name("Learning delivery funding and monitoring type – learning delivery monitoring (B)");
            Map(m => m.LearningDeliveryFamTypeLearningDeliveryMonitoringC).Index(i++).Name("Learning delivery funding and monitoring type – learning delivery monitoring (C)");
            Map(m => m.LearningDeliveryFamTypeLearningDeliveryMonitoringD).Index(i++).Name("Learning delivery funding and monitoring type – learning delivery monitoring (D)");
            Map(m => m.LearningDeliveryFamTypeLearningDeliveryMonitoringE).Index(i++).Name("Learning delivery funding and monitoring type – learning delivery monitoring (E)");
            Map(m => m.LearningDeliveryFamTypeLearningDeliveryMonitoringF).Index(i++).Name("Learning delivery funding and monitoring type – learning delivery monitoring (F)");

            Map(m => m.ProviderSpecifiedDeliveryMonitoringA).Index(i++).Name("Provider specified delivery monitoring (A)");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringB).Index(i++).Name("Provider specified delivery monitoring (B)");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringC).Index(i++).Name("Provider specified delivery monitoring (C)");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringD).Index(i++).Name("Provider specified delivery monitoring (D)");

            Map(m => m.LearningDeliveryEndPointAssessmentOrganisation).Index(i++).Name("End point assessment organisation");

            Map(m => m.RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim).Index(i++).Name("Planned number of on programme installments for aim");

            Map(m => m.LearningDeliverySubContractedOrPartnershipUkprn).Index(i++).Name("Sub contracted or partnership Ukprn");

            Map(m => m.PaymentPriceEpisodeStartDate).Index(i++).Name("Price episode start date");

            Map(m => m.RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate).Index(i++).Name("Price episode actual end date");

            Map(m => m.FcsContractContractAllocationContractAllocationNumber).Index(i++).Name("Contract no");

            Map(m => m.PaymentFundingLineType).Index(i++).Name("Funding line type");

            Map(m => m.PaymentApprenticeshipContractType).Index(i++).Name("Learning delivery funding and monitoring type – apprenticeship contract type");

            Map(m => m.LearnerEmploymentStatusEmployerId).Index(i++).Name("Employer identifier on employment status date");

            Map(m => m.RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier).Index(i++).Name("Agreement identifier");

            Map(m => m.LearnerEmploymentStatus).Index(i++).Name("Employment status");
            Map(m => m.LearnerEmploymentStatusDate).Index(i++).Name("Employment status date");

            Map(m => m.AugustLevyPayments).Index(i++).Name("August (R01) levy payments");
            Map(m => m.AugustCoInvestmentPayments).Index(i++).Name("August (R01) co-investment payments");
            Map(m => m.AugustCoInvestmentDueFromEmployerPayments).Index(i++).Name("August (R01) co-investment (below band upper limit) due from employer");
            Map(m => m.AugustEmployerAdditionalPayments).Index(i++).Name("August (R01) employer additional payments");
            Map(m => m.AugustProviderAdditionalPayments).Index(i++).Name("August (R01) provider additional payments");
            Map(m => m.AugustApprenticeAdditionalPayments).Index(i++).Name("August (R01) apprentice additional payments");
            Map(m => m.AugustEnglishAndMathsPayments).Index(i++).Name("August (R01) English and maths payments");
            Map(m => m.AugustLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("August (R01) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.AugustTotalPayments).Index(i++).Name("August (R01) total payments");

            Map(m => m.SeptemberLevyPayments).Index(i++).Name("September (R02) levy payments");
            Map(m => m.SeptemberCoInvestmentPayments).Index(i++).Name("September (R02) co-investment payments");
            Map(m => m.SeptemberCoInvestmentDueFromEmployerPayments).Index(i++).Name("September (R02) co-investment (below band upper limit) due from employer");
            Map(m => m.SeptemberEmployerAdditionalPayments).Index(i++).Name("September (R02) employer additional payments");
            Map(m => m.SeptemberProviderAdditionalPayments).Index(i++).Name("September (R02) provider additional payments");
            Map(m => m.SeptemberApprenticeAdditionalPayments).Index(i++).Name("September (R02) apprentice additional payments");
            Map(m => m.SeptemberEnglishAndMathsPayments).Index(i++).Name("September (R02) English and maths payments");
            Map(m => m.SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("September (R02) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.SeptemberTotalPayments).Index(i++).Name("September (R02) total payments");

            Map(m => m.OctoberLevyPayments).Index(i++).Name("October (R03) levy payments");
            Map(m => m.OctoberCoInvestmentPayments).Index(i++).Name("October (R03) co-investment payments");
            Map(m => m.OctoberCoInvestmentDueFromEmployerPayments).Index(i++).Name("October (R03) co-investment (below band upper limit) due from employer");
            Map(m => m.OctoberEmployerAdditionalPayments).Index(i++).Name("October (R03) employer additional payments");
            Map(m => m.OctoberProviderAdditionalPayments).Index(i++).Name("October (R03) provider additional payments");
            Map(m => m.OctoberApprenticeAdditionalPayments).Index(i++).Name("October (R03) apprentice additional payments");
            Map(m => m.OctoberEnglishAndMathsPayments).Index(i++).Name("October (R03) English and maths payments");
            Map(m => m.OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("October (R03) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.OctoberTotalPayments).Index(i++).Name("October (R03) total payments");

            Map(m => m.NovemberLevyPayments).Index(i++).Name("November (R04) levy payments");
            Map(m => m.NovemberCoInvestmentPayments).Index(i++).Name("November (R04) co-investment payments");
            Map(m => m.NovemberCoInvestmentDueFromEmployerPayments).Index(i++).Name("November (R04) co-investment (below band upper limit) due from employer");
            Map(m => m.NovemberEmployerAdditionalPayments).Index(i++).Name("November (R04) employer additional payments");
            Map(m => m.NovemberProviderAdditionalPayments).Index(i++).Name("November (R04) provider additional payments");
            Map(m => m.NovemberApprenticeAdditionalPayments).Index(i++).Name("November (R04) apprentice additional payments");
            Map(m => m.NovemberEnglishAndMathsPayments).Index(i++).Name("November (R04) English and maths payments");
            Map(m => m.NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("November (R04) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.NovemberTotalPayments).Index(i++).Name("November (R04) total payments");

            Map(m => m.DecemberLevyPayments).Index(i++).Name("December (R05) levy payments");
            Map(m => m.DecemberCoInvestmentPayments).Name("December (R05) co-investment payments");
            Map(m => m.DecemberCoInvestmentDueFromEmployerPayments).Index(i++).Name("December (R05) co-investment (below band upper limit) due from employer");
            Map(m => m.DecemberEmployerAdditionalPayments).Index(i++).Name("December (R05) employer additional payments");
            Map(m => m.DecemberProviderAdditionalPayments).Index(i++).Name("December (R05) provider additional payments");
            Map(m => m.DecemberApprenticeAdditionalPayments).Index(i++).Name("December (R05) apprentice additional payments");
            Map(m => m.DecemberEnglishAndMathsPayments).Index(i++).Name("December (R05) English and maths payments");
            Map(m => m.DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("December (R05) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.DecemberTotalPayments).Index(i++).Name("December (R05) total payments");

            Map(m => m.JanuaryLevyPayments).Index(i++).Name("January (R06) levy payments");
            Map(m => m.JanuaryCoInvestmentPayments).Index(i++).Name("January (R06) co-investment payments");
            Map(m => m.JanuaryCoInvestmentDueFromEmployerPayments).Index(i++).Name("January (R06) co-investment (below band upper limit) due from employer");
            Map(m => m.JanuaryEmployerAdditionalPayments).Index(i++).Name("January (R06) employer additional payments");
            Map(m => m.JanuaryProviderAdditionalPayments).Index(i++).Name("January (R06) provider additional payments");
            Map(m => m.JanuaryApprenticeAdditionalPayments).Index(i++).Name("January (R06) apprentice additional payments");
            Map(m => m.JanuaryEnglishAndMathsPayments).Index(i++).Name("January (R06) English and maths payments");
            Map(m => m.JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("January (R06) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.JanuaryTotalPayments).Index(i++).Name("January (R06) total payments");

            Map(m => m.FebruaryLevyPayments).Index(i++).Name("February (R07) levy payments");
            Map(m => m.FebruaryCoInvestmentPayments).Index(i++).Name("February (R07) co-investment payments");
            Map(m => m.FebruaryCoInvestmentDueFromEmployerPayments).Index(i++).Name("February (R07) co-investment (below band upper limit) due from employer");
            Map(m => m.FebruaryEmployerAdditionalPayments).Index(i++).Name("February (R07) employer additional payments");
            Map(m => m.FebruaryProviderAdditionalPayments).Index(i++).Name("February (R07) provider additional payments");
            Map(m => m.FebruaryApprenticeAdditionalPayments).Index(i++).Name("February (R07) apprentice additional payments");
            Map(m => m.FebruaryEnglishAndMathsPayments).Index(i++).Name("February (R07) English and maths payments");
            Map(m => m.FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("February (R07) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.FebruaryTotalPayments).Index(i++).Name("February (R07) total payments");

            Map(m => m.MarchLevyPayments).Index(i++).Name("March (R08) levy payments");
            Map(m => m.MarchCoInvestmentPayments).Index(i++).Name("March (R08) co-investment payments");
            Map(m => m.MarchCoInvestmentDueFromEmployerPayments).Index(i++).Name("March (R08) co-investment (below band upper limit) due from employer");
            Map(m => m.MarchEmployerAdditionalPayments).Index(i++).Name("March (R08) employer additional payments");
            Map(m => m.MarchProviderAdditionalPayments).Index(i++).Name("March (R08) provider additional payments");
            Map(m => m.MarchApprenticeAdditionalPayments).Index(i++).Name("March (R08) apprentice additional payments");
            Map(m => m.MarchEnglishAndMathsPayments).Index(i++).Name("March (R08) English and maths payments");
            Map(m => m.MarchLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("March (R08) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.MarchTotalPayments).Index(i++).Name("March (R08) total payments");

            Map(m => m.AprilLevyPayments).Index(i++).Name("April (R09) levy payments");
            Map(m => m.AprilCoInvestmentPayments).Index(i++).Name("April (R09) co-investment payments");
            Map(m => m.AprilCoInvestmentDueFromEmployerPayments).Index(i++).Name("April (R09) co-investment (below band upper limit) due from employer");
            Map(m => m.AprilEmployerAdditionalPayments).Index(i++).Name("April (R09) employer additional payments");
            Map(m => m.AprilProviderAdditionalPayments).Index(i++).Name("April (R09) provider additional payments");
            Map(m => m.AprilApprenticeAdditionalPayments).Index(i++).Name("April (R09) apprentice additional payments");
            Map(m => m.AprilEnglishAndMathsPayments).Index(i++).Name("April (R09) English and maths payments");
            Map(m => m.AprilLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("April (R09) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.AprilTotalPayments).Index(i++).Name("April (R09) total payments");

            Map(m => m.MayLevyPayments).Index(i++).Name("May (R10) levy payments");
            Map(m => m.MayCoInvestmentPayments).Index(i++).Name("May (R10) co-investment payments");
            Map(m => m.MayCoInvestmentDueFromEmployerPayments).Index(i++).Name("May (R10) co-investment (below band upper limit) due from employer");
            Map(m => m.MayEmployerAdditionalPayments).Index(i++).Name("May (R10) employer additional payments");
            Map(m => m.MayProviderAdditionalPayments).Index(i++).Name("May (R10) provider additional payments");
            Map(m => m.MayApprenticeAdditionalPayments).Index(i++).Name("May (R10) apprentice additional payments");
            Map(m => m.MayEnglishAndMathsPayments).Index(i++).Name("May (R10) English and maths payments");
            Map(m => m.MayLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("May (R10) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.MayTotalPayments).Index(i++).Name("May (R10) total payments");

            Map(m => m.JuneLevyPayments).Index(i++).Name("June (R11) levy payments");
            Map(m => m.JuneCoInvestmentPayments).Index(i++).Name("June (R11) co-investment payments");
            Map(m => m.JuneCoInvestmentDueFromEmployerPayments).Index(i++).Name("June (R11) co-investment (below band upper limit) due from employer");
            Map(m => m.JuneEmployerAdditionalPayments).Index(i++).Name("June (R11) employer additional payments");
            Map(m => m.JuneProviderAdditionalPayments).Index(i++).Name("June (R11) provider additional payments");
            Map(m => m.JuneApprenticeAdditionalPayments).Index(i++).Name("June (R11) apprentice additional payments");
            Map(m => m.JuneEnglishAndMathsPayments).Index(i++).Name("June (R11) English and maths payments");
            Map(m => m.JuneLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("June (R11) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.JuneTotalPayments).Index(i++).Name("June (R11) total payments");

            Map(m => m.JulyLevyPayments).Index(i++).Name("July (R12) levy payments");
            Map(m => m.JulyCoInvestmentPayments).Index(i++).Name("July (R12) co-investment payments");
            Map(m => m.JulyCoInvestmentDueFromEmployerPayments).Index(i++).Name("July (R12) co-investment (below band upper limit) due from employer");
            Map(m => m.JulyEmployerAdditionalPayments).Index(i++).Name("July (R12) employer additional payments");
            Map(m => m.JulyProviderAdditionalPayments).Index(i++).Name("July (R12) provider additional payments");
            Map(m => m.JulyApprenticeAdditionalPayments).Index(i++).Name("July (R12) apprentice additional payments");
            Map(m => m.JulyEnglishAndMathsPayments).Index(i++).Name("July (R12) English and maths payments");
            Map(m => m.JulyLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("July (R12) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.JulyTotalPayments).Index(i++).Name("July (R12) total payments");

            Map(m => m.R13LevyPayments).Index(i++).Name("R13 levy payments");
            Map(m => m.R13CoInvestmentPayments).Index(i++).Name("R13 co-investment payments");
            Map(m => m.R13CoInvestmentDueFromEmployerPayments).Index(i++).Name("R13 co-investment (below band upper limit) due from employer");
            Map(m => m.R13EmployerAdditionalPayments).Index(i++).Name("R13 employer additional payments");
            Map(m => m.R13ProviderAdditionalPayments).Index(i++).Name("R13 provider additional payments");
            Map(m => m.R13ApprenticeAdditionalPayments).Index(i++).Name("R13 apprentice additional payments");
            Map(m => m.R13EnglishAndMathsPayments).Index(i++).Name("R13 English and maths payments");
            Map(m => m.R13LearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("R13 payments for learning support, disadvantage and framework uplifts");
            Map(m => m.R13TotalPayments).Index(i++).Name("R13 total payments");

            Map(m => m.R14LevyPayments).Index(i++).Name("R14 levy payments");
            Map(m => m.R14CoInvestmentPayments).Index(i++).Name("R14 co-investment payments");
            Map(m => m.R14CoInvestmentDueFromEmployerPayments).Index(i++).Name("R14 co-investment (below band upper limit) due from employer");
            Map(m => m.R14EmployerAdditionalPayments).Index(i++).Name("R14 employer additional payments");
            Map(m => m.R14ProviderAdditionalPayments).Index(i++).Name("R14 provider additional payments");
            Map(m => m.R14ApprenticeAdditionalPayments).Index(i++).Name("R14 apprentice additional payments");
            Map(m => m.R14EnglishAndMathsPayments).Index(i++).Name("R14 English and maths payments");
            Map(m => m.R14LearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("R14 payments for learning support, disadvantage and framework uplifts");
            Map(m => m.R14TotalPayments).Index(i++).Name("R14 total payments");

            Map(m => m.TotalLevyPayments).Index(i++).Name("Total levy payments");
            Map(m => m.TotalCoInvestmentPayments).Index(i++).Name("Total co-investment payments");
            Map(m => m.TotalCoInvestmentDueFromEmployerPayments).Index(i++).Name("Total co-investment (below band upper limit) due from employer");
            Map(m => m.TotalEmployerAdditionalPayments).Index(i++).Name("Total employer additional payments");
            Map(m => m.TotalProviderAdditionalPayments).Index(i++).Name("Total provider additional payments");
            Map(m => m.TotalApprenticeAdditionalPayments).Index(i++).Name("Total apprentice additional payments");
            Map(m => m.TotalEnglishAndMathsPayments).Index(i++).Name("Total English and maths payments");
            Map(m => m.TotalLearningSupportDisadvantageAndFrameworkUpliftPayments).Index(i++).Name("Total payments for learning support, disadvantage and framework uplifts");
            Map(m => m.TotalPayments).Index(i++).Name("Total payments");
            Map(m => m.OfficialSensitive).Index(i++).Name("OFFICIAL - SENSITIVE");
        }
    }
}
