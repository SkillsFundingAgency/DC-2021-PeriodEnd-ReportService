namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model
{
    public class PaymentPeriodLines
    {
        public decimal Levy { get; set; }

        public decimal CoInvestment { get; set; }

        public decimal CoInvestmentDueFromEmployer { get; set; }

        public decimal EmployerAdditionalPayments { get; set; }

        public decimal ProviderAdditionalPayments { get; set; }

        public decimal ApprenticeAdditionalPayments { get; set; }

        public decimal EnglishAndMathsPayments { get; set; }
        
        public decimal PaymentsForLearningSupportDisadvantageAndFrameworkUplifts { get; set; }

        public decimal Total { get; set; }
    }
}
