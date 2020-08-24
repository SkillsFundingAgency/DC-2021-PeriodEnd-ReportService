using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats.DataProvider
{
    public interface ICollectionStatsDataProvider
    {
        Task<IEnumerable<CollectionStatsModel>> ProvideAsync(int collectionYear, int periodNumber);
    }
}
