using ESFA.DC.PeriodEnd.ReportService.Reports.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments
{
    public class AppsAdditionalPaymentsClassMap : AbstractClassMap<AppsAdditionalPaymentReportModel>
    {
        public AppsAdditionalPaymentsClassMap()
        {
            MapIndex(m => m.RecordKey.LearnerReferenceNumber).Name("Learner reference number");
            MapIndex(m => m.RecordKey.Uln).Name("Unique learner number");

            MapIndex(m => m.FamilyName).Name("Family name");
            MapIndex(m => m.GivenNames).Name("Given names");
            MapIndex(m => m.ProviderSpecifiedLearnerMonitoringA).Name("Provider specified learner monitoring (A)");
            MapIndex(m => m.ProviderSpecifiedLearnerMonitoringB).Name("Provider specified learner monitoring (B)");

            MapIndex(m => m.RecordKey.LearnStartDate).Name("Learning start date");
            MapIndex(m => m.RecordKey.ReportingAimFundingLineType).Name("Funding line type");
            MapIndex(m => m.RecordKey.PaymentType).Name("Type of additional payment");
            MapIndex(m => m.RecordKey.EmployerName).Name("Employer name from apprenticeship service");
            MapIndex(m => m.RecordKey.EmployerId).Name("Employer identifier from ILR");

            MapIndex(m => m.EarningsAndPayments.AugustEarnings).Name(Earnings("August"));
            MapIndex(m => m.EarningsAndPayments.AugustR01Payments).Name(Payments("August", 1));
            MapIndex(m => m.EarningsAndPayments.SeptemberEarnings).Name(Earnings("September"));
            MapIndex(m => m.EarningsAndPayments.SeptemberR02Payments).Name(Payments("September", 2));
            MapIndex(m => m.EarningsAndPayments.OctoberEarnings).Name(Earnings("October"));
            MapIndex(m => m.EarningsAndPayments.OctoberR03Payments).Name(Payments("October", 3));
            MapIndex(m => m.EarningsAndPayments.NovemberEarnings).Name(Earnings("November"));
            MapIndex(m => m.EarningsAndPayments.NovemberR04Payments).Name(Payments("November", 4));
            MapIndex(m => m.EarningsAndPayments.DecemberEarnings).Name(Earnings("December"));
            MapIndex(m => m.EarningsAndPayments.DecemberR05Payments).Name(Payments("December", 5));
            MapIndex(m => m.EarningsAndPayments.JanuaryEarnings).Name(Earnings("January"));
            MapIndex(m => m.EarningsAndPayments.JanuaryR06Payments).Name(Payments("January", 6));
            MapIndex(m => m.EarningsAndPayments.FebruaryEarnings).Name(Earnings("February"));
            MapIndex(m => m.EarningsAndPayments.FebruaryR07Payments).Name(Payments("February", 7));
            MapIndex(m => m.EarningsAndPayments.MarchEarnings).Name(Earnings("March"));
            MapIndex(m => m.EarningsAndPayments.MarchR08Payments).Name(Payments("March", 8));
            MapIndex(m => m.EarningsAndPayments.AprilEarnings).Name(Earnings("April"));
            MapIndex(m => m.EarningsAndPayments.AprilR09Payments).Name(Payments("April", 9));
            MapIndex(m => m.EarningsAndPayments.MayEarnings).Name(Earnings("May"));
            MapIndex(m => m.EarningsAndPayments.MayR10Payments).Name(Payments("May", 10));
            MapIndex(m => m.EarningsAndPayments.JuneEarnings).Name(Earnings("June"));
            MapIndex(m => m.EarningsAndPayments.JuneR11Payments).Name(Payments("June", 11));
            MapIndex(m => m.EarningsAndPayments.JulyEarnings).Name(Earnings("July"));
            MapIndex(m => m.EarningsAndPayments.JulyR12Payments).Name(Payments("July", 12));

            MapIndex(m => m.EarningsAndPayments.R13Payments).Name("R13 payments");
            MapIndex(m => m.EarningsAndPayments.R14Payments).Name("R14 payments");

            MapIndex(m => m.EarningsAndPayments.TotalEarnings).Name("Total earnings");
            MapIndex(m => m.EarningsAndPayments.TotalPaymentsYearToDate).Name("Total payments (year to date)");

            MapIndex().Constant(string.Empty).Name("OFFICIAL - SENSITIVE");
        }

        private string Earnings(string month) => $"{month} earnings";
        private string Payments(string month, int period) => $"{month} (R{period:D2}) payments";
    }
}