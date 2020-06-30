using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Persistence
{
    public interface IAppsCoInvestmentPersistenceService
    {
        Task PersistAsync(IReportServiceContext reportServiceContext, List<AppsCoInvestmentRecord> appsCoInvestmentRecords, CancellationToken cancellationToken);
    }
}
