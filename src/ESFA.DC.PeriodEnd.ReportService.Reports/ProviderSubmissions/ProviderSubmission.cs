using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.FileService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.ProviderSubmissions
{
    public class ProviderSubmission : IReport
    {
        private readonly IFileNameService _fileNameService;
        private readonly IExcelFileService _excelFileService;
        private readonly IProviderSubmissionsDataProvider _providerSubmissionsDataProvider;
        private readonly IProviderSubmissionsModelBuilder _providerSubmissionsModelBuilder;
        private readonly IProviderSubmissionsRenderService _providerSubmissionsRenderService;

        public string ReportTaskName => "TaskGenerateProviderSubmissionsReport";
        public bool IncludeInZip => false;

        private string ReportName = "ILR Provider Submissions Report";
        private string TemplateName = "ProviderSubmissionsReportTemplate.xlsx";
        private string WorksheetName = "Provider Submissions";

        public ProviderSubmission(IFileNameService fileNameService, IExcelFileService excelFileService, IProviderSubmissionsDataProvider providerSubmissionsDataProvider, IProviderSubmissionsModelBuilder providerSubmissionsModelBuilder, IProviderSubmissionsRenderService providerSubmissionsRenderService)
        {
            _fileNameService = fileNameService;
            _excelFileService = excelFileService;
            _providerSubmissionsDataProvider = providerSubmissionsDataProvider;
            _providerSubmissionsModelBuilder = providerSubmissionsModelBuilder;
            _providerSubmissionsRenderService = providerSubmissionsRenderService;
        }

        public async Task<string> GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var fileName = _fileNameService.GetInternalFilename(reportServiceContext, ReportName, OutputTypes.Excel);

            var referenceData = await _providerSubmissionsDataProvider.ProvideAsync(reportServiceContext);

            var models = _providerSubmissionsModelBuilder.Build(referenceData);

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(TemplateName));

            using (Stream manifestResourceStream = assembly.GetManifestResourceStream(resourceName))
            using (var workbook = _excelFileService.GetWorkbookFromTemplate(manifestResourceStream))
            {
                var worksheet = _excelFileService.GetWorksheetFromWorkbook(workbook, WorksheetName);

                _providerSubmissionsRenderService.Render(reportServiceContext.ReturnPeriod, models, worksheet, workbook);

                await _excelFileService.SaveWorkbookAsync(workbook, fileName, reportServiceContext.Container, cancellationToken);
            }

            return fileName;
        }
    }
}
