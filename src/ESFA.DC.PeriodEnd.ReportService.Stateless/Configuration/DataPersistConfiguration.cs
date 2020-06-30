using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;

namespace ESFA.DC.PeriodEnd.ReportService.Stateless.Configuration
{
    public class DataPersistConfiguration : IDataPersistConfiguration
    {
        public string ReportDataConnectionString { get; set; }

        public string DataPersistFeatureEnabled { get; set; }
    }
}
