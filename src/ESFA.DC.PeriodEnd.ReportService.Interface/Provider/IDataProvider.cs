using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IDataProvider
    {
        Task<object> ProvideAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
