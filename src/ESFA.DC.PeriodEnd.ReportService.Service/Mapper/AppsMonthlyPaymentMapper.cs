using System.Globalization;
using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Mapper
{
    public class AppsMonthlyPaymentMapper : ClassMap<AppsMonthlyPaymentModel>
    {
        public AppsMonthlyPaymentMapper()
        {
            int i = 0;
            Map(m => m.LearnerReferenceNumber).Index(i++).Name("Learner reference number");
            Map(m => m.UniqueLearnerNumber).Index(i++).Name("Unique learner number");
            Map(m => m.CampusIdentifier).Index(i++).Name("Campus identifier");
            Map(m => m.ProviderSpecifiedLearnerMonitoringA).Index(i++).Name("Provider specified learner monitoring (A)");
            Map(m => m.ProviderSpecifiedLearnerMonitoringB).Index(i++).Name("Provider specified learner monitoring (B)");
            Map(m => m.AimSeqNumber).Index(i++).Name("Aim sequence number");
            Map(m => m.LearningAimReference).Index(i++).Name("Learning aim reference");
            Map(m => m.LearningAimTitle).Index(i++).Name("Learning aim title");
            Map(m => m.LearningStartDate).Index(i++).Name("Learning start date");
            Map(m => m.LearningAimProgrammeType).Index(i++).Name("Programme type");
            Map(m => m.LearningAimStandardCode).Index(i++).Name("Standard code");
            Map(m => m.LearningAimFrameworkCode).Index(i++).Name("Framework code");
            Map(m => m.LearningAimPathwayCode).Index(i++).Name("Apprenticeship pathway");
            Map(m => m.AimType).Index(i++).Name("Aim type");
            Map(m => m.SoftwareSupplierAimIdentifier).Index(i++).Name("Software supplier aim identifier");
            Map(m => m.LearningDeliveryFAMTypeLearningDeliveryMonitoringA).Index(i++).Name("Learning delivery funding and monitoring type – learning delivery monitoring (A)");
            Map(m => m.LearningDeliveryFAMTypeLearningDeliveryMonitoringB).Index(i++).Name("Learning delivery funding and monitoring type – learning delivery monitoring (B)");
            Map(m => m.LearningDeliveryFAMTypeLearningDeliveryMonitoringC).Index(i++).Name("Learning delivery funding and monitoring type – learning delivery monitoring (C)");
            Map(m => m.LearningDeliveryFAMTypeLearningDeliveryMonitoringD).Index(i++).Name("Learning delivery funding and monitoring type – learning delivery monitoring (D)");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringA).Index(i++).Name("Provider specified delivery monitoring (A)");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringB).Index(i++).Name("Provider specified delivery monitoring (B)");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringC).Index(i++).Name("Provider specified delivery monitoring (C)");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringD).Index(i++).Name("Provider specified delivery monitoring (D)");
            Map(m => m.EndPointAssessmentOrganisation).Index(i++).Name("End point assessment organisation");
            Map(m => m.SubContractedOrPartnershipUKPRN).Index(i++).Name("Sub contracted or partnership UKPRN");
            Map(m => m.PriceEpisodeStartDate).Index(i++).Name("Price episode start date");
            Map(m => m.PriceEpisodeActualEndDate).Index(i++).Name("Price episode actual end date");
            Map(m => m.FundingLineType).Index(i++).Name("Funding line type");
            Map(m => m.LearningDeliveryFAMTypeApprenticeshipContractType).Index(i++).Name("Learning delivery funding and monitoring type – apprenticeship contract type");
            Map(m => m.AgreementIdentifier).Index(i++).Name("Agreement identifier");

            Map(m => m.AugustLevyPayments).ConvertUsing(model => model.LevyPayments[0].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("August (R01) levy payments");
            Map(m => m.AugustCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[0].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("August (R01) co-investment payments");
            Map(m => m.AugustCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[0].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("August (R01) co-investment (below band upper limit) due from employer");
            Map(m => m.AugustEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[0].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("August (R01) employer additional payments");
            Map(m => m.AugustProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[0].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("August (R01) provider additional payments");
            Map(m => m.AugustApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[0].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("August (R01) apprentice additional payments");
            Map(m => m.AugustEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[0].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("August (R01) English and maths payments");
            Map(m => m.AugustPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[0].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("August (R01) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.AugustTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[0].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("August (R01) total payments");

            Map(m => m.SeptemberLevyPayments).ConvertUsing(model => model.LevyPayments[1].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("September (R02) levy payments");
            Map(m => m.SeptemberCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[1].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("September (R02) co-investment payments");
            Map(m => m.SeptemberCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[1].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("September (R02) co-investment (below band upper limit) due from employer");
            Map(m => m.SeptemberEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[1].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("September (R02) employer additional payments");
            Map(m => m.SeptemberProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[1].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("September (R02) provider additional payments");
            Map(m => m.SeptemberApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[1].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("September (R02) apprentice additional payments");
            Map(m => m.SeptemberEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[1].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("September (R02) English and maths payments");
            Map(m => m.SeptemberPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[1].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("September (R02) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.SeptemberTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[1].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("September (R02) total payments");

            Map(m => m.OctoberLevyPayments).ConvertUsing(model => model.LevyPayments[2].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("October (R03) levy payments");
            Map(m => m.OctoberCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[2].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("October (R03) co-investment payments");
            Map(m => m.OctoberCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[2].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("October (R03) co-investment (below band upper limit) due from employer");
            Map(m => m.OctoberEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[2].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("October (R03) employer additional payments");
            Map(m => m.OctoberProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[2].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("October (R03) provider additional payments");
            Map(m => m.OctoberApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[2].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("October (R03) apprentice additional payments");
            Map(m => m.OctoberEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[2].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("October (R03) English and maths payments");
            Map(m => m.OctoberPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[2].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("October (R03) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.OctoberTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[2].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("October (R03) total payments");

            Map(m => m.NovemberLevyPayments).ConvertUsing(model => model.LevyPayments[3].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("November (R04) levy payments");
            Map(m => m.NovemberCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[3].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("November (R04) co-investment payments");
            Map(m => m.NovemberCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[3].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("November (R04) co-investment (below band upper limit) due from employer");
            Map(m => m.NovemberEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[3].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("November (R04) employer additional payments");
            Map(m => m.NovemberProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[3].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("November (R04) provider additional payments");
            Map(m => m.NovemberApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[3].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("November (R04) apprentice additional payments");
            Map(m => m.NovemberEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[3].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("November (R04) English and maths payments");
            Map(m => m.NovemberPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[3].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("November (R04) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.NovemberTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[3].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("November (R04) total payments");

            Map(m => m.DecemberLevyPayments).ConvertUsing(model => model.LevyPayments[4].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("December (R05) levy payments");
            Map(m => m.DecemberCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[4].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("December (R05) co-investment payments");
            Map(m => m.DecemberCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[4].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("December (R05) co-investment (below band upper limit) due from employer");
            Map(m => m.DecemberEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[4].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("December (R05) employer additional payments");
            Map(m => m.DecemberProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[4].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("December (R05) provider additional payments");
            Map(m => m.DecemberApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[4].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("December (R05) apprentice additional payments");
            Map(m => m.DecemberEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[4].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("December (R05) English and maths payments");
            Map(m => m.DecemberPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[4].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("December (R05) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.DecemberTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[4].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("December (R05) total payments");

            Map(m => m.JanuaryLevyPayments).ConvertUsing(model => model.LevyPayments[5].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("January (R06) levy payments");
            Map(m => m.JanuaryCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[5].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("January (R06) co-investment payments");
            Map(m => m.JanuaryCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[5].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("January (R06) co-investment (below band upper limit) due from employer");
            Map(m => m.JanuaryEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[5].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("January (R06) employer additional payments");
            Map(m => m.JanuaryProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[5].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("January (R06) provider additional payments");
            Map(m => m.JanuaryApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[5].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("January (R06) apprentice additional payments");
            Map(m => m.JanuaryEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[5].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("January (R06) English and maths payments");
            Map(m => m.JanuaryPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[5].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("January (R06) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.JanuaryTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[5].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("January (R06) total payments");

            Map(m => m.FebruaryLevyPayments).ConvertUsing(model => model.LevyPayments[6].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("February (R07) levy payments");
            Map(m => m.FebruaryCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[6].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("February (R07) co-investment payments");
            Map(m => m.FebruaryCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[6].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("February (R07) co-investment (below band upper limit) due from employer");
            Map(m => m.FebruaryEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[6].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("February (R07) employer additional payments");
            Map(m => m.FebruaryProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[6].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("February (R07) provider additional payments");
            Map(m => m.FebruaryApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[6].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("February (R07) apprentice additional payments");
            Map(m => m.FebruaryEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[6].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("February (R07) English and maths payments");
            Map(m => m.FebruaryPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[6].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("February (R07) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.FebruaryTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[6].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("February (R07) total payments");

            Map(m => m.MarchLevyPayments).ConvertUsing(model => model.LevyPayments[7].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("March (R08) levy payments");
            Map(m => m.MarchCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[7].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("March (R08) co-investment payments");
            Map(m => m.MarchCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[7].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("March (R08) co-investment (below band upper limit) due from employer");
            Map(m => m.MarchEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[7].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("March (R08) employer additional payments");
            Map(m => m.MarchProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[7].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("March (R08) provider additional payments");
            Map(m => m.MarchApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[7].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("March (R08) apprentice additional payments");
            Map(m => m.MarchEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[7].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("March (R08) English and maths payments");
            Map(m => m.MarchPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[7].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("March (R08) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.MarchTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[7].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("March (R08) total payments");

            Map(m => m.AprilLevyPayments).ConvertUsing(model => model.LevyPayments[8].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("April (R09) levy payments");
            Map(m => m.AprilCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[8].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("April (R09) co-investment payments");
            Map(m => m.AprilCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[8].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("April (R09) co-investment (below band upper limit) due from employer");
            Map(m => m.AprilEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[8].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("April (R09) employer additional payments");
            Map(m => m.AprilProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[8].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("April (R09) provider additional payments");
            Map(m => m.AprilApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[8].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("April (R09) apprentice additional payments");
            Map(m => m.AprilEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[8].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("April (R09) English and maths payments");
            Map(m => m.AprilPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[8].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("April (R09) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.AprilTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[8].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("April (R09) total payments");

            Map(m => m.MayLevyPayments).ConvertUsing(model => model.LevyPayments[9].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("May (R10) levy payments");
            Map(m => m.MayCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[9].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("May (R10) co-investment payments");
            Map(m => m.MayCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[9].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("May (R10) co-investment (below band upper limit) due from employer");
            Map(m => m.MayEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[9].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("May (R10) employer additional payments");
            Map(m => m.MayProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[9].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("May (R10) provider additional payments");
            Map(m => m.MayApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[9].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("May (R10) apprentice additional payments");
            Map(m => m.MayEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[9].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("May (R10) English and maths payments");
            Map(m => m.MayPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[9].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("May (R10) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.MayTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[9].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("May (R10) total payments");

            Map(m => m.JuneLevyPayments).ConvertUsing(model => model.LevyPayments[10].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("June (R11) levy payments");
            Map(m => m.JuneCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[10].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("June (R11) co-investment payments");
            Map(m => m.JuneCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[10].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("June (R11) co-investment (below band upper limit) due from employer");
            Map(m => m.JuneEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[10].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("June (R11) employer additional payments");
            Map(m => m.JuneProviderAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[10].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("June (R11) provider additional payments");
            Map(m => m.JuneApprenticeAdditionalPayments).ConvertUsing(model => model.EnglishAndMathsPayments[10].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("June (R11) apprentice additional payments");
            Map(m => m.JuneEnglishAndMathsPayments).ConvertUsing(model => model.PaymentsForLearningSupport[10].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("June (R11) English and maths payments");
            Map(m => m.JunePaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[10].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("June (R11) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.JuneTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[10].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("June (R11) total payments");

            Map(m => m.JulyLevyPayments).ConvertUsing(model => model.LevyPayments[11].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("July (R12) levy payments");
            Map(m => m.JulyCoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[11].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("July (R12) co-investment payments");
            Map(m => m.JulyCoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[11].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("July (R12) co-investment (below band upper limit) due from employer");
            Map(m => m.JulyEmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[11].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("July (R12) employer additional payments");
            Map(m => m.JulyProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[11].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("July (R12) provider additional payments");
            Map(m => m.JulyApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[11].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("July (R12) apprentice additional payments");
            Map(m => m.JulyEnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[11].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("July (R12) English and maths payments");
            Map(m => m.JulyPaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[11].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("July (R12) payments for learning support, disadvantage and framework uplifts");
            Map(m => m.JulyTotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[11].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("July (R12) total payments");

            Map(m => m.R13LevyPayments).ConvertUsing(model => model.LevyPayments[12].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R13 levy payments");
            Map(m => m.R13CoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[12].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R13 co-investment payments");
            Map(m => m.R13CoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[12].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R13 co-investment (below band upper limit) due from employer");
            Map(m => m.R13EmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[12].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R13 employer additional payments");
            Map(m => m.R13ProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[12].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R13 provider additional payments");
            Map(m => m.R13ApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[12].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R13 apprentice additional payments");
            Map(m => m.R13EnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[12].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R13 English and maths payments");
            Map(m => m.R13PaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[12].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R13 payments for learning support, disadvantage and framework uplifts");
            Map(m => m.R13TotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[12].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R13 total payments");

            Map(m => m.R14LevyPayments).ConvertUsing(model => model.LevyPayments[13].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R14 levy payments");
            Map(m => m.R14CoInvestmentPayments).ConvertUsing(model => model.CoInvestmentPayments[13].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R14 co-investment payments");
            Map(m => m.R14CoInvestmentDueFromEmployerPayments).ConvertUsing(model => model.CoInvestmentDueFromEmployerPayments[13].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R14 co-investment (below band upper limit) due from employer");
            Map(m => m.R14EmployerAdditionalPayments).ConvertUsing(model => model.EmployerAdditionalPayments[13].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R14 employer additional payments");
            Map(m => m.R14ProviderAdditionalPayments).ConvertUsing(model => model.ProviderAdditionalPayments[13].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R14 provider additional payments");
            Map(m => m.R14ApprenticeAdditionalPayments).ConvertUsing(model => model.ApprenticeAdditionalPayments[13].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R14 apprentice additional payments");
            Map(m => m.R14EnglishAndMathsPayments).ConvertUsing(model => model.EnglishAndMathsPayments[13].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R14 English and maths payments");
            Map(m => m.R14PaymentsForLearningSupport).ConvertUsing(model => model.PaymentsForLearningSupport[13].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R14 payments for learning support, disadvantage and framework uplifts");
            Map(m => m.R14TotalPayments).ConvertUsing(model => model.TotalMonthlyPayments[13].ToString(CultureInfo.InvariantCulture)).Index(i++).Name("R14 total payments");

            Map(m => m.TotalLevyPayments).Index(i++).Name("Total levy payments");
            Map(m => m.TotalCoInvestmentPayments).Index(i++).Name("Total co-investment payments");
            Map(m => m.TotalCoInvestmentDueFromEmployerPayments).Index(i++).Name("Total co-investment (below band upper limit) due from employer");
            Map(m => m.TotalEmployerAdditionalPayments).Index(i++).Name("Total employer additional payments");
            Map(m => m.TotalProviderAdditionalPayments).Index(i++).Name("Total provider additional payments");
            Map(m => m.TotalApprenticeAdditionalPayments).Index(i++).Name("Total apprentice additional payments");
            Map(m => m.TotalEnglishAndMathsPayments).Index(i++).Name("Total English and maths payments");
            Map(m => m.TotalPaymentsForLearningSupport).Index(i++).Name("Total payments for learning support, disadvantage and framework uplifts");
            Map(m => m.TotalPayments).Index(i++).Name("Total payments");
            Map(m => m.OfficialSensitive).Index(i++).Name("OFFICIAL - SENSITIVE");
        }
    }
}
