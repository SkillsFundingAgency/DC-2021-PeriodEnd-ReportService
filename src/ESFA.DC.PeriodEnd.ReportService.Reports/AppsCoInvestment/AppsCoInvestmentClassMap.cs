using ESFA.DC.PeriodEnd.ReportService.Reports.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment
{
    public class AppsCoInvestmentClassMap : AbstractClassMap<AppsCoInvestmentRecord>
    {
        public AppsCoInvestmentClassMap()
        {
            MapIndex(m => m.RecordKey.LearnerReferenceNumber).Name("Learner reference number");
            MapIndex(m => m.UniqueLearnerNumber).Name("Unique learner number");
            MapIndex(m => m.FamilyName).Name("Family name");
            MapIndex(m => m.GivenNames).Name("Given names");
            MapIndex(m => m.RecordKey.LearningStartDate).Name("Learning start date");
            MapIndex(m => m.RecordKey.ProgrammeType).Name("Programme type");
            MapIndex(m => m.RecordKey.StandardCode).Name("Standard code");
            MapIndex(m => m.RecordKey.FrameworkCode).Name("Framework code");
            MapIndex(m => m.RecordKey.PathwayCode).Name("Apprenticeship pathway");
            MapIndex(m => m.LearningDelivery.SWSupAimId).Name("Software supplier aim identifier");
            MapIndex(m => m.ApprenticeshipContractType).Name("Learning delivery funding and monitoring type - apprenticeship contract type");
            MapIndex(m => m.EmployerIdentifierAtStartOfLearning).Name("Employer identifier (ERN) at start of learning");
            MapIndex(m => m.EmployerNameFromApprenticeshipService).Name("Employer name from apprenticeship service");
            MapIndex(m => m.EarningsAndPayments.EmployerCoInvestmentPercentage).Name("Employer co-investment percentage");
            MapIndex(m => m.AecLearningDelivery.AppAdjLearnStartDate).Name("Applicable programme start date");
            MapIndex(m => m.EarningsAndPayments.TotalPMRPreviousFundingYears).Name("Total employer contribution collected (PMR) in previous funding years");
            MapIndex(m => m.EarningsAndPayments.TotalCoInvestmentDueFromEmployerInPreviousFundingYears).Name("Total co-investment (below band upper limit) due from employer in previous funding years");
            MapIndex(m => m.EarningsAndPayments.TotalPMRThisFundingYear).Name("Total employer contribution collected (PMR) in this funding year");
            MapIndex(m => m.EarningsAndPayments.TotalCoInvestmentDueFromEmployerThisFundingYear).Name("Total co-investment (below band upper limit) due from employer in this funding year");
            MapIndex(m => m.EarningsAndPayments.PercentageOfCoInvestmentCollected).Name("Percentage of co-investment collected (for all funding years)").TypeConverterOption.Format("0.00");
            MapIndex(m => m.LDM356Or361).Name("LDM 356 or 361?");
            MapIndex(m => m.EarningsAndPayments.CompletionEarningThisFundingYear).Name("Completion earnings in this funding year");
            MapIndex(m => m.EarningsAndPayments.CompletionPaymentsThisFundingYear).Name("Completion payments in this funding year (including employer co-investment)");
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.August).Name(CoInvestment("August (R01)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.September).Name(CoInvestment("September (R02)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.October).Name(CoInvestment("October (R03)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.November).Name(CoInvestment("November (R04)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.December).Name(CoInvestment("December (R05)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.January).Name(CoInvestment("January (R06)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.February).Name(CoInvestment("February (R07)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.March).Name(CoInvestment("March (R08)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.April).Name(CoInvestment("April (R09)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.May).Name(CoInvestment("May (R10)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.June).Name(CoInvestment("June (R11)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.July).Name(CoInvestment("July (R12)"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.R13).Name(CoInvestment("R13"));
            MapIndex(m => m.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.R14).Name(CoInvestment("R14"));
            MapIndex().Constant(string.Empty).Name("OFFICIAL - SENSITIVE");
        }

        private string CoInvestment(string period) => $"Co-investment (below band upper limit) due from employer for {period}";
    }
}
