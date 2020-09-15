﻿using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.CrossYearPayments
{
    public class CrossYearPayments : IReport
    {
        private readonly IExcelFileService _excelFileService;
        private readonly IFileNameService _fileNameService;
        private readonly ICrossYearModelBuilder _crossYearModelBuilder;
        private readonly ICrossYearDataProvider _crossYearDataProvider;
        private readonly ICrossYearRenderService _crossYearRenderService;

        public string ReportTaskName => "TaskGenerateCrossYearPaymentsReport";

        public bool IncludeInZip => true;

        private const string TemplateName = "CrossYearPaymentsReportTemplate.xlsx";
        private const string ReportName = "Cross Year Indicative Payments Report";
        private const string WorksheetName = "Sheet1";

        public CrossYearPayments(IExcelFileService excelFileService, IFileNameService fileNameService, ICrossYearModelBuilder crossYearModelBuilder, ICrossYearDataProvider crossYearDataProvider, ICrossYearRenderService crossYearRenderService)
        {
            _excelFileService = excelFileService;
            _fileNameService = fileNameService;
            _crossYearModelBuilder = crossYearModelBuilder;
            _crossYearDataProvider = crossYearDataProvider;
            _crossYearRenderService = crossYearRenderService;
        }

        public async Task<string> GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var fileName = _fileNameService.GetFilename(reportServiceContext, ReportName, OutputTypes.Excel);

            var data = await _crossYearDataProvider.ProvideAsync(reportServiceContext, cancellationToken);

            var model = _crossYearModelBuilder.Build(data, reportServiceContext);

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(TemplateName));

            using (Stream manifestResourceStream = assembly.GetManifestResourceStream(resourceName))
            using (var workbook = _excelFileService.GetWorkbookFromTemplate(manifestResourceStream))
            {
                var worksheet = _excelFileService.GetWorksheetFromWorkbook(workbook, WorksheetName);

                _crossYearRenderService.Render(model, worksheet, workbook);

                await _excelFileService.SaveWorkbookAsync(workbook, fileName, reportServiceContext.Container, cancellationToken);
            }

            return fileName;
        }
    }
}