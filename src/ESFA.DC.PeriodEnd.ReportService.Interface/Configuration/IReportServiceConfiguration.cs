﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Configuration
{
    public interface IReportServiceConfiguration
    {
        string DASPaymentsConnectionString { get; set; }

        string ILR1920DataStoreConnectionString { get; set; }

        string FCSConnectionString { get; set; }

        string LarsConnectionString { get; set; }
    }
}
