using System.Collections.Generic;
using Aspose.Cells;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions
{
    public interface IProviderSubmissionsRenderService
    {
        Worksheet Render(int periodNumber, ICollection<ProviderSubmissionModel> providerSubmissionModels, Worksheet worksheet, Workbook workbook);
    }
}
