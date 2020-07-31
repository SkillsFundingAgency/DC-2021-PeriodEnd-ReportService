using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.CollectionStats
{
    public class CollectionStatsModelBuilder : ICollectionStatsModelBuilder
    {
        private readonly ICollectionStatsDataProvider _collectionStatsDataProvider;

        public CollectionStatsModelBuilder(ICollectionStatsDataProvider collectionStatsDataProvider)
        {
            _collectionStatsDataProvider = collectionStatsDataProvider;
        }

        public async Task<IEnumerable<CollectionStatsModel>> BuildAsync(int collectionYear, int periodNumber) => await _collectionStatsDataProvider.ProvideAsync(collectionYear, periodNumber);

    }
}
