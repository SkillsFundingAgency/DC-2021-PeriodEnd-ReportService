using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.DataQuality
{
    public class DataQuality : IReport
    {
        private readonly IFileNameService _fileNameService;
        private readonly IExcelFileService _excelFileService;
        private readonly IDataQualityDataProvider _dataQualityDataProvider;
        private readonly IDataQualityModelBuilder _dataQualityModelBuilder;
        private readonly IDataQualityRenderService _dataQualityRenderService;

        private string ReportName = "Data Quality Report - Test";
        private string WorksheetName = "Data Quality";
        private string TemplateName = "ILRDataQualityReportTemplate.xlsx";

        public string ReportTaskName => "TaskGenerateDataQualityReport";

        public bool IncludeInZip => false;

        public DataQuality(IFileNameService fileNameService, IExcelFileService excelFileService, IDataQualityDataProvider dataQualityDataProvider, IDataQualityModelBuilder dataQualityModelBuilder, IDataQualityRenderService dataQualityRenderService)
        {
            _fileNameService = fileNameService;
            _excelFileService = excelFileService;
            _dataQualityDataProvider = dataQualityDataProvider;
            _dataQualityModelBuilder = dataQualityModelBuilder;
            _dataQualityRenderService = dataQualityRenderService;
        }

        public async Task<string> GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var fileName = _fileNameService.GetInternalFilename(reportServiceContext, ReportName, OutputTypes.Excel);

            var data = await _dataQualityDataProvider.ProvideAsync(reportServiceContext, cancellationToken);

            var model = _dataQualityModelBuilder.Build(data, reportServiceContext);

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(TemplateName));

            using (Stream manifestResourceStream = assembly.GetManifestResourceStream(resourceName))
            using (var workbook = _excelFileService.GetWorkbookFromTemplate(manifestResourceStream))
            {
                var worksheet = _excelFileService.GetWorksheetFromWorkbook(workbook, WorksheetName);

                _dataQualityRenderService.Render(reportServiceContext.ReturnPeriodName, model, worksheet, workbook);

                await _excelFileService.SaveWorkbookAsync(workbook, fileName, reportServiceContext.Container, cancellationToken);
            }

            return fileName;
        }
    }
}
