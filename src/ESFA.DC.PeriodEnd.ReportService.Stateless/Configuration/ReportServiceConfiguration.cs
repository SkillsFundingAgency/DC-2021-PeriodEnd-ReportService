using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;

namespace ESFA.DC.PeriodEnd.ReportService.Stateless.Configuration
{
    public class ReportServiceConfiguration : IReportServiceConfiguration
    {
        public string DASPaymentsConnectionString { get; set; }

        public string FCSConnectionString { get; set; }

        public string ILRDataStoreConnectionString { get; set; }

        public string ILRDataStoreValidConnectionString { get; set; }

        public string LarsConnectionString { get; set; }

        public string OrgConnectionString { get; set; }

        public string SummarisedActualsConnectionString { get; set; }

        public string JobQueueManagerConnectionString { get; set; }
    }
}
