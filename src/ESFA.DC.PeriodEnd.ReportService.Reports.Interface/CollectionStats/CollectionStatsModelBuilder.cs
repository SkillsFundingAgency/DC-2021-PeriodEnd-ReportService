using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats
{
    public interface ICollectionStatsModelBuilder
    {
        Task<IEnumerable<CollectionStatsModel>> BuildAsync(int collectionYear, int periodNumber);
    }
}
