using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Interface.Context;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IReportsProvider
    {
        IEnumerable<IReport> ProvideReportsForContext(IReportServiceContext reportServiceContext);
    }
}
