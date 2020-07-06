using System.Collections.Generic;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Persistence
{
    public interface IAppsCoInvestmentPersistenceMapper
    {
        IEnumerable<AppsCoInvestmentContribution> Map(IReportServiceContext reportServiceContext, IEnumerable<AppsCoInvestmentRecord> appsCoInvestmentRecords, CancellationToken cancellationToken);
    }
}
