using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Persistence
{
    public interface IAppsMonthlyPersistenceMapper
    {
        IEnumerable<AppsMonthlyPayment> Map(IReportServiceContext reportServiceContext, IEnumerable<AppsMonthlyRecord> appsMonthlyRecords, CancellationToken cancellationToken);
    }
}
