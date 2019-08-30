using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.PeriodEndMetrics;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess
{
    public interface IPeriodEndQueryService1920
    {
        Task<IEnumerable<IlrMetrics>> GetPeriodEndMetrics(int periodId);
    }
}