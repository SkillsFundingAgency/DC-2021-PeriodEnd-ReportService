using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments
{
    public interface ICrossYearModelBuilder
    {
        CrossYearPaymentsModel Build(CrossYearDataModel dataModel, IReportServiceContext reportServiceContext);
    }
}
