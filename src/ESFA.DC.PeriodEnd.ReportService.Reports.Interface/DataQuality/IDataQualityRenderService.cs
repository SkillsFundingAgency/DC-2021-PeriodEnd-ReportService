using Aspose.Cells;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality
{
    public interface IDataQualityRenderService
    {
        Worksheet Render(string periodNumberName, DataQualityProviderModel dataQualityProvideModel, Worksheet worksheet, Workbook workbook);
    }
}
