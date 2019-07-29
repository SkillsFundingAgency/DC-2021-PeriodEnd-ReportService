using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Mapper
{
    public class AppsAdditionalPaymentsMapper : ClassMap<AppsAdditionalPaymentsModel>
    {
        public AppsAdditionalPaymentsMapper()
        {
            int i = 0;
            Map(m => m.LearnerReferenceNumber).Index(i++).Name("Learner reference number");
            Map(m => m.UniqueLearnerNumber).Index(i++).Name("Unique learner number");
            Map(m => m.ProviderSpecifiedLearnerMonitoringA).Index(i++).Name("Provider specified learner monitoring (A)");
            Map(m => m.ProviderSpecifiedLearnerMonitoringB).Index(i++).Name("Provider specified learner monitoring (B)");
            Map(m => m.LearningStartDate).Index(i++).Name("Learning start date");
            Map(m => m.FundingLineType).Index(i++).Name("Funding line type");
            Map(m => m.TypeOfAdditionalPayment).Index(i++).Name("Type of additional payment");
            Map(m => m.EmployerNameFromApprenticeshipService).Index(i++).Name("Employer name from apprenticeship service");
            Map(m => m.EmployerIdentifierFromILR).Index(i++).Name("Employer identifier from ILR");

            Map(m => m.AugustEarnings).Index(i++).Name("August earnings");
            Map(m => m.AugustR01Payments).Index(i++).Name("August (R01) payments");
            Map(m => m.SeptemberEarnings).Index(i++).Name("September earnings");
            Map(m => m.SeptemberR02Payments).Index(i++).Name("September (R02) payments");
            Map(m => m.OctoberEarnings).Index(i++).Name("October earnings");
            Map(m => m.OctoberR03Payments).Index(i++).Name("October (R03) payments");
            Map(m => m.NovemberEarnings).Index(i++).Name("November earnings");
            Map(m => m.NovemberR04Payments).Index(i++).Name("November (R04) payments");
            Map(m => m.DecemberEarnings).Index(i++).Name("December earnings");
            Map(m => m.DecemberR05Payments).Index(i++).Name("December (R05) payments");
            Map(m => m.JanuaryEarnings).Index(i++).Name("January earnings");
            Map(m => m.JanuaryR06Payments).Index(i++).Name("January (R06) payments");
            Map(m => m.FebruaryEarnings).Index(i++).Name("February earnings");
            Map(m => m.FebruaryR07Payments).Index(i++).Name("February (R07) payments");
            Map(m => m.MarchEarnings).Index(i++).Name("March earnings");
            Map(m => m.MarchR08Payments).Index(i++).Name("March (R08) payments");
            Map(m => m.AprilEarnings).Index(i++).Name("April earnings");
            Map(m => m.AprilR09Payments).Index(i++).Name("April (R09) payments");
            Map(m => m.MayEarnings).Index(i++).Name("May earnings");
            Map(m => m.MayR10Payments).Index(i++).Name("May (R10) payments");
            Map(m => m.JuneEarnings).Index(i++).Name("June earnings");
            Map(m => m.JuneR11Payments).Index(i++).Name("June (R11) Payments");
            Map(m => m.JulyEarnings).Index(i++).Name("July earnings");
            Map(m => m.JulyR12Payments).Index(i++).Name("July (R12) payments");

            Map(m => m.R13Payments).Index(i++).Name("R13 payments");
            Map(m => m.R14Payments).Index(i++).Name("R14 payments");
            Map(m => m.TotalEarnings).Index(i++).Name("Total earnings");
            Map(m => m.TotalPaymentsYearToDate).Index(i++).Name("Total payments (year to date)");

            Map(m => m.OfficialSensitive).Index(i++).Name("OFFICIAL - SENSITIVE");
        }
    }
}
