using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IFM36ProviderService
    {
        Task<FM36Global> GetFM36Data(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}