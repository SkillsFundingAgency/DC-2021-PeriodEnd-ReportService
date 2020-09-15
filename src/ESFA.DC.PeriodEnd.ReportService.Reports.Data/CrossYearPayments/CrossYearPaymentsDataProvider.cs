using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.CrossYearPayments
{
    public class CrossYearPaymentsDataProvider : ICrossYearDataProvider
    {
        public async Task<CrossYearDataModel> ProvideAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new CrossYearDataModel());
        }
    }
}
