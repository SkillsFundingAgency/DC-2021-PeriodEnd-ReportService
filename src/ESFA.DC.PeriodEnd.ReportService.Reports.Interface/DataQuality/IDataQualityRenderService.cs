using Aspose.Cells;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality
{
    public interface IDataQualityRenderService
    {
        Worksheet Render(int periodNumber, DataQualityProviderModel dataQualityProvideModel, Worksheet worksheet, Workbook workbook);
    }
}
