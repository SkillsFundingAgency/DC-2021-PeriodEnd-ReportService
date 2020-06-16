using Aspose.Cells;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface
{
    public interface IRenderService<T>
    {
        Worksheet Render(T model, Worksheet worksheet);
    }
}
