using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.PeriodEndMetrics;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Mappers
{
    public class PeriodEndMetricsEarningsMapper : ClassMap<IlrMetrics>
    {
        public PeriodEndMetricsEarningsMapper()
        {
            int i = 0;
            Map(m => m.TransactionType).Index(i++);
            Map(m => m.EarningsYTD).Index(i++);
            Map(m => m.EarningsACT1).Index(i++);
            Map(m => m.EarningsACT2).Index(i++);
        }
    }
}