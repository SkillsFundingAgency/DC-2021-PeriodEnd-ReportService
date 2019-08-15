using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Mappers
{
    public class PeriodEndMetricsPaymentsMapper : ClassMap<PaymentMetrics>
    {
        public PeriodEndMetricsPaymentsMapper()
        {
            int i = 0;
            Map(m => m.TransactionType).Index(i++);
            Map(m => m.EarningsYTD).Index(i++);
            Map(m => m.EarningsACT1).Index(i++);
            Map(m => m.EarningsACT2).Index(i++);
            Map(m => m.NegativeEarnings).Index(i++);
            Map(m => m.NegativeEarningsACT1).Index(i++);
            Map(m => m.NegativeEarningsACT2).Index(i++);
            Map(m => m.PaymentsYTD).Index(i++);
            Map(m => m.PaymentsACT1).Index(i++);
            Map(m => m.PaymentsACT2).Index(i++);
            Map(m => m.DataLockErrors).Index(i++);
            Map(m => m.HeldBackCompletion).Index(i++);
            Map(m => m.HBCPACT1).Index(i++);
            Map(m => m.HBCPACT2).Index(i++);
        }
    }
}