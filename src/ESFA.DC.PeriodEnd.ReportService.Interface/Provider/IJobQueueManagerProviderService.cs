using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IJobQueueManagerProviderService
    {
        Task<int> GetCollectionIdAsync(string collectionType, CancellationToken cancellationToken);

        Task<IEnumerable<OrganisationCollectionModel>> GetExpectedReturnersUKPRNsAsync(
            int CollectionId,
            CancellationToken cancellationToken);

        Task<IEnumerable<long>> GetActualReturnersUKPRNsAsync(
            int collectionId,
            int returnPeriod,
            CancellationToken cancellationToken);

        Task<IEnumerable<CollectionStatsModel>> GetCollectionStatsModels(
            int collectionYear,
            int collectionPeriod,
            CancellationToken cancellationToken);

        Task<IEnumerable<ProviderReturnPeriod>> GetReturnersAndPeriodsAsync(int collectionId, int returnPeriod, CancellationToken cancellationToken);
    }
}
