using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Configuration
{
    public interface IReportServiceConfiguration
    {
        string DasCommitmentsConnectionString { get; set; }

        string DASPaymentsConnectionString { get; set; }

        string ILRDataStoreConnectionString { get; set; }

        string FCSConnectionString { get; set; }

        string LarsConnectionString { get; set; }
    }
}
