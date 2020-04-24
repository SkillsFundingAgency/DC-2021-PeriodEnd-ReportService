using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IReportsProvider
    {
        IEnumerable<ILegacyReport> ProvideReportsForContext(IReportServiceContext reportServiceContext);
    }
}
