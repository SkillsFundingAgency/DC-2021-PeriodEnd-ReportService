using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider
{
    public interface IDataQualityDataProvider
    {
        Task<DataQualityProviderModel> ProvideAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
