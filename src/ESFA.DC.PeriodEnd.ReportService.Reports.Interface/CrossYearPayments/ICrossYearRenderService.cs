using Aspose.Cells;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments
{
    public interface ICrossYearRenderService
    {
        Worksheet Render(IReportServiceContext reportServiceContext, CrossYearPaymentsModel model, Worksheet worksheet, Workbook workbook);
    }
}
