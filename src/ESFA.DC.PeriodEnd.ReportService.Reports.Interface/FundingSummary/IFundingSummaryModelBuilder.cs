using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary
{
    public interface IFundingSummaryModelBuilder
    {
        Task<FundingSummaryReportModel> Build(IReportServiceContext reportServiceContext, IFundingSummaryDataModel fundingSummaryDataModel, CancellationToken cancellationToken);
    }
}
