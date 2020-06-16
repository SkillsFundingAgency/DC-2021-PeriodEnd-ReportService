using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Persistance;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.FundingSummary
{
    public class FundingSummaryPersistanceService : IFundingSummaryPersistanceService
    {
        private readonly IFundingSummaryPersistanceMapper _fundingSummaryPersistanceMapper;

        public FundingSummaryPersistanceService(IFundingSummaryPersistanceMapper fundingSummaryPersistanceMapper)
        {
            _fundingSummaryPersistanceMapper = fundingSummaryPersistanceMapper;
        }

        public async Task PersistAsync(IReportServiceContext reportServiceContext, FundingSummaryReportModel fundingSummaryReportModel, CancellationToken cancellationToken)
        {
            if (reportServiceContext.DataPersistFeatureEnabled)
            {
                var model = _fundingSummaryPersistanceMapper.MapAsync(reportServiceContext, fundingSummaryReportModel, cancellationToken);
            }
        }
    }
}
