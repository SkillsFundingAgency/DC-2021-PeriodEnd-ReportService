using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments
{
    public interface ICrossYearDataProvider
    {
        Task<CrossYearDataModel> ProvideAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
