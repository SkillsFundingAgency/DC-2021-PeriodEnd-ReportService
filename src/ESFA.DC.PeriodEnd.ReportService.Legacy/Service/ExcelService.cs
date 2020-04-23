using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;
using ESFA.DC.FileService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Service
{
    public class ExcelService : IExcelService
    {
        private readonly IFileService _fileService;

        public ExcelService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public Workbook NewWorkbook() => new Workbook();

        public Worksheet GetWorksheetFromWorkbook(Workbook workbook, string sheetName) => workbook.Worksheets[sheetName] ?? workbook.Worksheets.Add(sheetName);

        public Worksheet GetWorksheetFromWorkbook(Workbook workbook, int index) => workbook.Worksheets[index];

        public async Task SaveWorkbookAsync(Workbook workbook, string fileName, string container, CancellationToken cancellationToken)
        {
            using (Stream ms = await _fileService.OpenWriteStreamAsync(fileName, container, cancellationToken))
            {
                workbook.Save(ms, SaveFormat.Xlsx);
            }
        }

        public void ApplyLicense()
        {
            const string licenseResource = "ESFA.DC.PeriodEnd.ReportService.Service.Reports.Resources.Aspose.Cells.lic";
            var workbook = new Workbook();

            if (!workbook.IsLicensed)
            {
                var license = new License();

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(licenseResource))
                {
                    license.SetLicense(stream);
                }
            }
        }
    }
}