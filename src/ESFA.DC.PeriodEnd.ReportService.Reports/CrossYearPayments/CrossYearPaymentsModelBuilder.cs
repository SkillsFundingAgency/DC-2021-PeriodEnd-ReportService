using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.CrossYearPayments
{
    public class CrossYearPaymentsModelBuilder : ICrossYearModelBuilder
    {
        public CrossYearPaymentsModel Build(CrossYearDataModel dataModel, IReportServiceContext reportServiceContext)
        {
            return new CrossYearPaymentsModel();
        }
    }
}
