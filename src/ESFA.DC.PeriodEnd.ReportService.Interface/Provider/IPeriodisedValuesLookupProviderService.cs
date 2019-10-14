using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IPeriodisedValuesLookupProviderService
    {
        Task<IPeriodisedValuesLookup> ProvideAsync(CancellationToken cancellationToken);
    }
}