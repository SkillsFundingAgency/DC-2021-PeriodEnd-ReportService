using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Data;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider
{
    public interface IFundingSummaryDataProvider
    {
        Task<IFundingSummaryDataModel> ProvideAsync(IReportServiceContext reportServiceContext,
            CancellationToken cancellationToken);
    }
}
