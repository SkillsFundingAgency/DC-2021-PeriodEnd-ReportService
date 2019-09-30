using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IFm35PeriodEndProviderService
    {
        Dictionary<string, Dictionary<string, decimal?[][]>> GetFM35LearningDeliveryPerioisedValues(int ukprn);
    }
}