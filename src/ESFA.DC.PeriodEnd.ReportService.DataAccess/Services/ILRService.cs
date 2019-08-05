using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.ILR.DataService.Interfaces.Services;
using ESFA.DC.ILR.DataService.Models.PeriodEnd;

namespace ESFA.DC.PeriodEnd.ReportService.DataAccess.Services
{
    public class Ilr1920MetricsService
    {
        private readonly IPeriodEndMetricsService1920 _ilrMetricsService;
        

        public Ilr1920MetricsService(IPeriodEndMetricsService1920 ilrMetricsService)
        {
            _ilrMetricsService = ilrMetricsService;
            
        }

        public async Task<IEnumerable<PeriodEndMetrics>> GetPeriodEndMetrics(int periodId)
        {
            var metrics = await _ilrMetricsService.GetPeriodEndMetrics(periodId);

            return metrics;
        }
    }
}