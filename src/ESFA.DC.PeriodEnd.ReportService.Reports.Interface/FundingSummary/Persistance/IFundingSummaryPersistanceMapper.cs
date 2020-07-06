using System.Collections.Generic;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Persistance
{
    public interface IFundingSummaryPersistanceMapper
    {
        IEnumerable<FundingSummaryReport> Map(IReportServiceContext reportServiceContext, FundingSummaryReportModel fundingSummaryReportModel,CancellationToken cancellationToken);
    }
}
