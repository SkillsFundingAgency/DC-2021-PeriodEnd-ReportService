namespace ESFA.DC.PeriodEnd.ReportService.DataAccess.Contexts
{
    public class PaymentMetricsEntity
    {
        public int TransactionType { get; set; }

        public decimal? EarningsYTD { get; set; }

        public decimal? EarningsACT1 { get; set; }

        public decimal? EarningsACT2 { get; set; }

        public decimal? NegativeEarnings { get; set; }

        public decimal? NegativeEarningsACT1 { get; set; }

        public decimal? NegativeEarningsACT2 { get; set; }

        public decimal? PaymentsYTD { get; set; }

        public decimal? PaymentsACT1 { get; set; }

        public decimal? PaymentsACT2 { get; set; }

        public decimal? DataLockErrors { get; set; }

        public decimal? HeldBackCompletion { get; set; }

        public decimal? HBCPACT1 { get; set; }

        public decimal? HBCPACT2 { get; set; }
    }
}