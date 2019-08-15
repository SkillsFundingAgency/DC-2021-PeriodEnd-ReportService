using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestmentContributions;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Mapper
{
    public sealed class AppsCoInvestmentContributionsMapper : ClassMap<AppsCoInvestmentContributionsModel>
    {
        public AppsCoInvestmentContributionsMapper()
        {
            int i = 0;
            Map(m => m.LearnRefNumber).Index(i++).Name("Learner reference number");
            Map(m => m.UniqueLearnerNumber).Index(i++).Name("Unique learner number");
            Map(m => m.LearningStartDate).Index(i++).Name("Learning start date");
            Map(m => m.ProgType).Index(i++).Name("Programme type");
            Map(m => m.StandardCode).Index(i++).Name("Standard code");
            Map(m => m.FrameworkCode).Index(i++).Name("Framework code");
            Map(m => m.ApprenticeshipPathway).Index(i++).Name("Apprenticeship pathway");
            Map(m => m.SoftwareSupplierAimIdentifier).Index(i++).Name("Software supplier aim identifier");
            Map(m => m.LearningDeliveryFAMTypeApprenticeshipContractType).Index(i++).Name("Learning delivery funding and monitoring type - apprenticeship contract type");
            Map(m => m.EmployerIdentifierAtStartOfLearning).Index(i++).Name("Employer identifier (ERN) at start of learning");
            Map(m => m.EmployerNameFromApprenticeshipService).Index(i++).Name("Employer name from apprenticeship service");
            Map(m => m.EmployerCoInvestmentPercentage).Index(i++).Name("Employer co-investment percentage");
            Map(m => m.EmployerProgrammeStartDate).Index(i++).Name("Applicable programme start date");
            Map(m => m.TotalPMRPreviousFundingYears).Index(i++).Name("Total employer contribution collected (PMR) in previous funding years");
            Map(m => m.TotalCoInvestmentDueFromEmployerInPreviousFundingYears).Index(i++).Name("Total co-investment (below band upper limit) due from employer in previous funding years");
            Map(m => m.TotalPMRThisFundingYear).Index(i++).Name("Total employer contribution collected (PMR) in this funding year");
            Map(m => m.TotalCoInvestmentDueFromEmployerThisFundingYear).Index(i++).Name("Total co-investment (below band upper limit) due from employer in this funding year");
            Map(m => m.PercentageOfCoInvestmentCollected).Index(i++).Name("Percentage of co-investment collected (for all funding years)");
            Map(m => m.LDM356Or361).Index(i++).Name("LDM 356 or 361?");
            Map(m => m.CompletionEarningThisFundingYear).Index(i++).Name("Completion earnings in this funding year");
            Map(m => m.CompletionPaymentsThisFundingYear).Index(i++).Name("Completion payments in this funding year");
            Map(m => m.CoInvestmentDueFromEmployerForAugust).Index(i++).Name("Co-investment (below band upper limit) due from employer for August (R01)");
            Map(m => m.CoInvestmentDueFromEmployerForSeptember).Index(i++).Name("Co-investment (below band upper limit) due from employer for September (R02)");
            Map(m => m.CoInvestmentDueFromEmployerForOctober).Index(i++).Name("Co-investment (below band upper limit) due from employer for October (R03)");
            Map(m => m.CoInvestmentDueFromEmployerForNovember).Index(i++).Name("Co-investment (below band upper limit) due from employer for November (R04)");
            Map(m => m.CoInvestmentDueFromEmployerForDecember).Index(i++).Name("Co-investment (below band upper limit) due from employer for December (R05)");
            Map(m => m.CoInvestmentDueFromEmployerForJanuary).Index(i++).Name("Co-investment (below band upper limit) due from employer for January (R06)");
            Map(m => m.CoInvestmentDueFromEmployerForFebruary).Index(i++).Name("Co-investment (below band upper limit) due from employer for February (R07)");
            Map(m => m.CoInvestmentDueFromEmployerForMarch).Index(i++).Name("Co-investment (below band upper limit) due from employer for March (R08)");
            Map(m => m.CoInvestmentDueFromEmployerForApril).Index(i++).Name("Co-investment (below band upper limit) due from employer for April (R09)");
            Map(m => m.CoInvestmentDueFromEmployerForMay).Index(i++).Name("Co-investment (below band upper limit) due from employer for May (R10)");
            Map(m => m.CoInvestmentDueFromEmployerForJune).Index(i++).Name("Co-investment (below band upper limit) due from employer for June (R11)");
            Map(m => m.CoInvestmentDueFromEmployerForJuly).Index(i++).Name("Co-investment (below band upper limit) due from employer for July (R12)");
            Map(m => m.CoInvestmentDueFromEmployerForR13).Index(i++).Name("Co-investment (below band upper limit) due from employer for R13");
            Map(m => m.CoInvestmentDueFromEmployerForR14).Index(i++).Name("Co-investment (below band upper limit) due from employer for R14");
            Map(m => m.OfficialSensitive).Index(i).Name("OFFICIAL - SENSITIVE");
        }
    }
}
