using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IJobQueueManagerProviderService
    {
        Task<IEnumerable<long>> GetExpectedReturnersUKPRNsAsync(
            string collectionName,
            int returnPeriod,
            IEnumerable<ReturnPeriod> returnPeriods,
            CancellationToken cancellationToken);

        Task<IEnumerable<long>> GetActualReturnersUKPRNsAsync(
            string collectionName,
            int returnPeriod,
            IEnumerable<ReturnPeriod> returnPeriods,
            CancellationToken cancellationToken);

        Task<IEnumerable<CollectionStatsModel>> GetCollectionStatsModels(
            int collectionYear,
            int collectionPeriod,
            CancellationToken cancellationToken);
    }
}
