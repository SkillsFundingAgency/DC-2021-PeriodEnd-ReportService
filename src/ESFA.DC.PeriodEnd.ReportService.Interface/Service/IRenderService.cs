using Aspose.Cells;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IRenderService<T>
    {
        Worksheet Render(T model, Worksheet worksheet);
    }
}