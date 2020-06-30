using System.Collections.Generic;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Persistence.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Persistence
{
    public interface IAppsCoInvestmentPersistenceMapper
    {
        IEnumerable<AppsCoInvestmentPersistModel> MapAsync(IReportServiceContext reportServiceContext, IEnumerable<AppsCoInvestmentRecord> appsCoInvestmentRecords, CancellationToken cancellationToken);
    }
}
