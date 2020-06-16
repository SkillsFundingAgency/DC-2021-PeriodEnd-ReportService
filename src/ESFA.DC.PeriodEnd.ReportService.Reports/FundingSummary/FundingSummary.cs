using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Persistance;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.FundingSummary
{
    public class FundingSummary : IReport
    {
        private readonly IExcelFileService _excelFileService;
        private readonly IFileNameService _fileNameService;
        private readonly IFundingSummaryDataProvider _fundingSummaryDataProvider;
        private readonly IFundingSummaryModelBuilder _fundingSummaryModelBuilder;
        private readonly IRenderService<FundingSummaryReportModel> _fundingSummaryRenderService;
        private readonly IFundingSummaryPersistanceService _fundingSummaryPersistanceService;

        public string ReportTaskName => "TaskGenerateFundingSummaryReport";

        private string ReportName => "Funding Summary Report";

        public FundingSummary(IExcelFileService excelFileService, IFileNameService fileNameService, IFundingSummaryDataProvider fundingSummaryDataProvider, IFundingSummaryModelBuilder fundingSummaryModelBuilder, IRenderService<FundingSummaryReportModel> fundingSummaryRenderService, IFundingSummaryPersistanceService fundingSummaryPersistanceService)
        {
            _excelFileService = excelFileService;
            _fileNameService = fileNameService;
            _fundingSummaryDataProvider = fundingSummaryDataProvider;
            _fundingSummaryModelBuilder = fundingSummaryModelBuilder;
            _fundingSummaryRenderService = fundingSummaryRenderService;
            _fundingSummaryPersistanceService = fundingSummaryPersistanceService;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var fileName = _fileNameService.GetFilename(reportServiceContext, ReportName, OutputTypes.Excel, true);

            var fundingSummaryReferenceData = await _fundingSummaryDataProvider.ProvideAsync(reportServiceContext, cancellationToken);

            var model = await _fundingSummaryModelBuilder.Build(reportServiceContext, fundingSummaryReferenceData, cancellationToken);

            using (var workbook = _excelFileService.NewWorkbook())
            {
                workbook.Worksheets.Clear();

                var worksheet = _excelFileService.GetWorksheetFromWorkbook(workbook, "FundingSummaryReport");

                _fundingSummaryRenderService.Render(model, worksheet);

                await _excelFileService.SaveWorkbookAsync(workbook, fileName, reportServiceContext.Container, cancellationToken);
            }

            await _fundingSummaryPersistanceService.PersistAsync(reportServiceContext, model, cancellationToken);
        }
    }
}
