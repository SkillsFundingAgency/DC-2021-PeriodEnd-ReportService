﻿using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;

namespace ESFA.DC.PeriodEnd.ReportService.Stateless.Configuration
{
    public class ReportServiceConfiguration : IReportServiceConfiguration
    {
        public string DASPaymentsConnectionString { get; set; }

        public string ILR1920DataStoreConnectionString { get; set; }

        public string FCSConnectionString { get; set; }

        public string LarsConnectionString { get; set; }

        public string SummarisedActualsConnectionString { get; set; }
    }
}
