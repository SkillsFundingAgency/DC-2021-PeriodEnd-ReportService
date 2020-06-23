using System;
using System.Linq.Expressions;
using System.Text;
using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class AppsMonthlyClassMap : AbstractClassMap<AppsMonthlyRecord>
    {
        private string _period;

        private const string R01 = "August (R01)";
        private const string R02 = "September (R02)";
        private const string R03 = "October (R03)";
        private const string R04 = "November (R04)";
        private const string R05 = "December (R05)";
        private const string R06 = "January (R06)";
        private const string R07 = "February (R07)";
        private const string R08 = "March (R08)";
        private const string R09 = "April (R09)";
        private const string R10 = "May (R10)";
        private const string R11 = "June (R11)";
        private const string R12 = "July (R12)";
        private const string R13 = "R13";
        private const string R14 = "R14";
        private const string TotalString = "Total";

        public AppsMonthlyClassMap()
        {
            MapIndex(m => m.RecordKey.LearnerReferenceNumber).Name("Learner reference number");
            MapIndex(m => m.RecordKey.Uln).Name("Unique learner number");

            MapIndex(m => m.FamilyName).Name("Family name");
            MapIndex(m => m.GivenNames).Name("Given names");

            MapIndex(m => m.Learner.CampusIdentifier).Name("Campus identifier");

            MapIndex(m => m.ProviderMonitorings.LearnerA).Name("Provider specified learner monitoring (A)");
            MapIndex(m => m.ProviderMonitorings.LearnerB).Name("Provider specified learner monitoring (B)");

            MapIndex(m => m.Earning.AimSequenceNumber).Name("Aim sequence number");

            MapIndex(m => m.RecordKey.LearningAimReference).Name("Learning aim reference");

            MapIndex(m => m.LearningDeliveryTitle).Name("Learning aim title");

            MapIndex(m => m.LearningDelivery.OrigLearnStartDate).Name("Original learning start date");

            MapIndex(m => m.RecordKey.LearnStartDate).Name("Learning start date");

            MapIndex(m => m.LearningDelivery.LearnPlanEndDate).Name("Learning planned end date");
            MapIndex(m => m.LearningDelivery.CompStatus).Name("Completion status");
            MapIndex(m => m.LearningDelivery.LearnActEndDate).Name("Learning actual end date");
            MapIndex(m => m.LearningDelivery.AchDate).Name("Achievement date");
            MapIndex(m => m.LearningDelivery.Outcome).Name("Outcome");

            MapIndex(m => m.RecordKey.ProgrammeType).Name("Programme type");
            MapIndex(m => m.RecordKey.StandardCode).Name("Standard code");
            MapIndex(m => m.RecordKey.FrameworkCode).Name("Framework code");
            MapIndex(m => m.RecordKey.PathwayCode).Name("Apprenticeship pathway");

            MapIndex(m => m.LearningDelivery.AimType).Name("Aim type");
            MapIndex(m => m.LearningDelivery.SWSupAimId).Name("Software supplier aim identifier");

            MapIndex(m => m.LearningDeliveryFams.LDM1).Name("Learning delivery funding and monitoring type - learning delivery monitoring (A)");
            MapIndex(m => m.LearningDeliveryFams.LDM2).Name("Learning delivery funding and monitoring type - learning delivery monitoring (B)");
            MapIndex(m => m.LearningDeliveryFams.LDM3).Name("Learning delivery funding and monitoring type - learning delivery monitoring (C)");
            MapIndex(m => m.LearningDeliveryFams.LDM4).Name("Learning delivery funding and monitoring type - learning delivery monitoring (D)");
            MapIndex(m => m.LearningDeliveryFams.LDM5).Name("Learning delivery funding and monitoring type - learning delivery monitoring (E)");
            MapIndex(m => m.LearningDeliveryFams.LDM6).Name("Learning delivery funding and monitoring type - learning delivery monitoring (F)");

            MapIndex(m => m.ProviderMonitorings.LearningDeliveryA).Name("Provider specified delivery monitoring (A)");
            MapIndex(m => m.ProviderMonitorings.LearningDeliveryB).Name("Provider specified delivery monitoring (B)");
            MapIndex(m => m.ProviderMonitorings.LearningDeliveryC).Name("Provider specified delivery monitoring (C)");
            MapIndex(m => m.ProviderMonitorings.LearningDeliveryD).Name("Provider specified delivery monitoring (D)");

            MapIndex(m => m.LearningDelivery.EPAOrgId).Name("End point assessment organisation");
            MapIndex(m => m.LearningDelivery.AecLearningDelivery.PlannedNumOnProgInstalm).Name("Planned number of on programme instalments for aim");
            MapIndex(m => m.LearningDelivery.PartnerUkprn).Name("Sub contracted or partnership UKPRN");

            MapIndex(m => m.PriceEpisodeStartDate).Name("Price episode start date");

            MapIndex(m => m.PriceEpisode.PriceEpisodeActualEndDateIncEPA).Name("Price episode actual end date");

            MapIndex(m => m.ContractNumber).Name("Contract no");

            MapIndex(m => m.RecordKey.ReportingAimFundingLineType).Name("Funding line type");

            MapIndex(m => m.RecordKey.ContractType).Name("Learning delivery funding and monitoring type - apprenticeship contract type");

            MapIndex(m => m.LearnerEmploymentStatus.EmpId).Name("Employer identifier on employment status date");
            MapIndex(m => m.LearnerEmploymentStatus.EmpStat).Name("Employment status");
            MapIndex(m => m.LearnerEmploymentStatus.DateEmpStatApp).Name("Employment status date");

            _period = R01;

            MapIndex(m => m.PaymentPeriods.R01.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R01.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R01.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R01.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R01.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R01.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R01.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R01.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R01.Total).Name(Total(_period));

            _period = R02;

            MapIndex(m => m.PaymentPeriods.R02.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R02.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R02.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R02.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R02.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R02.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R02.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R02.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R02.Total).Name(Total(_period));

            _period = R03;

            MapIndex(m => m.PaymentPeriods.R03.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R03.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R03.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R03.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R03.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R03.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R03.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R03.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R03.Total).Name(Total(_period));

            _period = R04;

            MapIndex(m => m.PaymentPeriods.R04.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R04.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R04.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R04.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R04.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R04.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R04.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R04.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R04.Total).Name(Total(_period));

            _period = R05;

            MapIndex(m => m.PaymentPeriods.R05.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R05.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R05.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R05.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R05.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R05.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R05.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R05.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R05.Total).Name(Total(_period));

            _period = R06;

            MapIndex(m => m.PaymentPeriods.R06.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R06.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R06.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R06.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R06.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R06.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R06.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R06.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R06.Total).Name(Total(_period));

            _period = R07;

            MapIndex(m => m.PaymentPeriods.R07.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R07.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R07.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R07.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R07.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R07.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R07.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R07.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R07.Total).Name(Total(_period));

            _period = R08;

            MapIndex(m => m.PaymentPeriods.R08.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R08.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R08.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R08.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R08.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R08.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R08.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R08.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R08.Total).Name(Total(_period));

            _period = R09;

            MapIndex(m => m.PaymentPeriods.R09.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R09.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R09.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R09.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R09.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R09.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R09.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R09.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R09.Total).Name(Total(_period));

            _period = R10;

            MapIndex(m => m.PaymentPeriods.R10.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R10.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R10.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R10.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R10.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R10.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R10.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R10.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R10.Total).Name(Total(_period));

            _period = R11;

            MapIndex(m => m.PaymentPeriods.R11.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R11.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R11.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R11.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R11.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R11.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R11.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R11.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R11.Total).Name(Total(_period));

            _period = R12;

            MapIndex(m => m.PaymentPeriods.R12.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R12.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R12.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R12.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R12.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R12.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R12.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R12.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R12.Total).Name(Total(_period));

            _period = R13;

            MapIndex(m => m.PaymentPeriods.R13.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R13.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R13.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R13.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R13.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R13.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R13.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R13.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R13.Total).Name(Total(_period));

            _period = R14;

            MapIndex(m => m.PaymentPeriods.R14.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.R14.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.R14.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.R14.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R14.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R14.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.R14.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.R14.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.R14.Total).Name(Total(_period));

            _period = TotalString;

            MapIndex(m => m.PaymentPeriods.Total.Levy).Name(Levy(_period));
            MapIndex(m => m.PaymentPeriods.Total.CoInvestment).Name(CoInvestment(_period));
            MapIndex(m => m.PaymentPeriods.Total.CoInvestmentDueFromEmployer).Name(CoInvestmentDueFromEmployer(_period));
            MapIndex(m => m.PaymentPeriods.Total.EmployerAdditional).Name(EmployerAdditional(_period));
            MapIndex(m => m.PaymentPeriods.Total.ProviderAdditional).Name(ProviderAdditional(_period));
            MapIndex(m => m.PaymentPeriods.Total.ApprenticeAdditional).Name(ApprenticeAdditional(_period));
            MapIndex(m => m.PaymentPeriods.Total.EnglishAndMaths).Name(EnglishAndMaths(_period));
            MapIndex(m => m.PaymentPeriods.Total.LearningSupportDisadvantageAndFrameworkUplifts).Name(LearningSupportDisadvantageAndFrameworkUplifts(_period));
            MapIndex(m => m.PaymentPeriods.Total.Total).Name(Total(_period, true));
          

            MapIndex().Constant(string.Empty).Name("OFFICIAL - SENSITIVE");
        }

        private string Levy(string period) => $"{period} levy payments";

        private string CoInvestment(string period) => $"{period} co-investment payments";

        private string CoInvestmentDueFromEmployer(string period) => $"{period} co-investment (below band upper limit) due from employer";

        private string EmployerAdditional(string period) => $"{period} employer additional payments";

        private string ProviderAdditional(string period) => $"{period} provider additional payments";

        private string ApprenticeAdditional(string period) => $"{period} apprentice additional payments";

        private string EnglishAndMaths(string period) => $"{period} English and maths payments";

        private string LearningSupportDisadvantageAndFrameworkUplifts(string period) => $"{period} payments for learning support, disadvantage and framework uplifts";

        private string Total(string period, bool isTotal = false)
        {
            var totalPaymentsBuilder = new StringBuilder(period);
            
            if (!isTotal)
            {
                totalPaymentsBuilder.Append(" total");
            }

            totalPaymentsBuilder.Append(" payments");

            return totalPaymentsBuilder.ToString();
        }
    }
}
