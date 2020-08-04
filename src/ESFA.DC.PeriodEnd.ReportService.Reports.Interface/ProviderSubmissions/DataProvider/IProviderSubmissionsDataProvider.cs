using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.DataProvider
{
    public interface IProviderSubmissionsDataProvider
    {
        Task<ProviderSubmissionsReferenceData> ProvideAsync(IReportServiceContext reportServiceContext);
    }
}
