using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IExcelService
    {
        Workbook NewWorkbook();

        Worksheet GetWorksheetFromWorkbook(Workbook workbook, int index);

        Worksheet GetWorksheetFromWorkbook(Workbook workbook, string sheetName);

        Task SaveWorkbookAsync(Workbook workbook, string fileName, string container, CancellationToken cancellationToken);

        void ApplyLicense();
    }
}