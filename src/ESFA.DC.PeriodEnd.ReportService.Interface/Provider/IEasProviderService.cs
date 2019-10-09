using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport.Eas;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IEasProviderService
    {
        Task<IList<ProviderEasInfo>> GetProviderEasInfoForFundingSummaryReport(int ukPrn, CancellationToken cancellationToken);
    }
}