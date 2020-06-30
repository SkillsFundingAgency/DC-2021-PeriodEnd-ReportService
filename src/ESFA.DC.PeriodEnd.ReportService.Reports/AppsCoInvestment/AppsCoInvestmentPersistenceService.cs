using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Persistence;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment
{
    public class AppsCoInvestmentPersistenceService : IAppsCoInvestmentPersistenceService
    {
        private readonly IAppsCoInvestmentPersistenceMapper _appsCoInvestmentPersistenceMapper;

        public AppsCoInvestmentPersistenceService(IAppsCoInvestmentPersistenceMapper appsCoInvestmentPersistenceMapper)
        {
            _appsCoInvestmentPersistenceMapper = appsCoInvestmentPersistenceMapper;
        }
        public async Task PersistAsync(IReportServiceContext reportServiceContext, List<AppsCoInvestmentRecord> appsCoInvestmentRecords, CancellationToken cancellationToken)
        {
            if (reportServiceContext.DataPersistFeatureEnabled)
            {
                var model = _appsCoInvestmentPersistenceMapper.MapAsync(reportServiceContext, appsCoInvestmentRecords, cancellationToken);
            }
        }
    }
}
