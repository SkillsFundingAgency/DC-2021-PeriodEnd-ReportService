using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.ActCount
{
    public class ActCountModelBuilder : IActCountModelBuilder
    {
        private readonly IActCountDataProvider _actCountDataProvider;

        public ActCountModelBuilder(IActCountDataProvider actCountDataProvider)
        {
            _actCountDataProvider = actCountDataProvider;
        }

        public async Task<IEnumerable<ActCountModel>> BuildAsync(CancellationToken cancellationToken)
         => await _actCountDataProvider.ProvideAsync(cancellationToken);
    }
}
