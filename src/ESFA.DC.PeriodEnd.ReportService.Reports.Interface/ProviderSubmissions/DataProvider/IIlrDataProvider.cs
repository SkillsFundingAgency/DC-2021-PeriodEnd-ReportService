using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.DataProvider
{
    public interface IIlrDataProvider
    {
        Task<ICollection<FileDetails>> ProvideAsync(IEnumerable<ProviderReturnPeriod> providerReturnPeriods);
    }
}
