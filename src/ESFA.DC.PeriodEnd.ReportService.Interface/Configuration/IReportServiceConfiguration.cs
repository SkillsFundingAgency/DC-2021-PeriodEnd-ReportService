using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Configuration
{
    public interface IReportServiceConfiguration
    {
        string DASPaymentsConnectionString { get; set; }

        string FCSConnectionString { get; set; }

        string ILRDataStoreConnectionString { get; set; }

        string LarsConnectionString { get; set; }

        string SummarisedActualsConnectionString { get; set; }

        string JobQueueManagerConnectionString { get; set; }

        string EasConnectionString { get; set; }
    }
}
