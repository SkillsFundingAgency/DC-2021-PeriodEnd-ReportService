namespace ESFA.DC.PeriodEnd.ReportService.Model.InternalReports
{
    public class PaymentMetrics
    {
        public int TransactionType { get; set; }

        public int EarningsYTD { get; set; }

        public int EarningsACT1 { get; set; }

        public int EarningsACT2 { get; set; }

        public int NegativeEarnings { get; set; }

        public int NegativeEarningsACT1 { get; set; }

        public int NegativeEarningsACT2 { get; set; }

        public int PaymentsYTD { get; set; }

        public int PaymentsACT1 { get; set; }

        public int PaymentsACT2 { get; set; }

        public int DataLockErrors { get; set; }

        public int HeldBackCompletion { get; set; }

        public int HBCPACT1 { get; set; }

        public int HBCPACT2 { get; set; }
    }
}