using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Configuration
{
    public interface IPeriodEndReportServiceConfiguration
    {
        string DASPaymentsConnectionString { get; set; }

        string ILRDataStoreConnectionString { get; set; }

        string EasConnectionString { get; set; }

        string LarsConnectionString { get; set; }

//        string OrgConnectionString { get; set; }
    }
}
