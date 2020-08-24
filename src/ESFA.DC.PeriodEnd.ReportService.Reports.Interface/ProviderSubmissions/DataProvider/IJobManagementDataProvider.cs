using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.DataProvider
{
    public interface IJobManagementDataProvider
    {
        Task<int> ProvideCollectionIdAsync(string collectionType);

        Task<ICollection<ProviderReturnPeriod>> ProvideReturnersAndPeriodsAsync(int collectionId, int returnPeriod);

        Task<ICollection<OrganisationCollection>> ProvideExpectedReturnersUKPRNsAsync(int collectionId);

        Task<ICollection<long>> ProvideActualReturnersUKPRNsAsync(int collectionId, int returnPeriod);
    }
}
