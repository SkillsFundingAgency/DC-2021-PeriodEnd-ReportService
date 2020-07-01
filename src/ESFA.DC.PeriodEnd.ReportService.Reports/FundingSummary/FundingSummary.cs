using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Persistance;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Persistance.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.FundingSummary
{
    public class FundingSummary : IReport
    {
        private readonly IExcelFileService _excelFileService;
        private readonly IFileNameService _fileNameService;
        private readonly IFundingSummaryDataProvider _fundingSummaryDataProvider;
        private readonly IFundingSummaryModelBuilder _fundingSummaryModelBuilder;
        private readonly IRenderService<FundingSummaryReportModel> _fundingSummaryRenderService;
        private readonly IReportDataPersistanceService<FundingSummaryPersistModel> _persistanceService;
        private readonly IFundingSummaryPersistanceMapper _fundingSummaryPersistanceMapper;

        public string ReportTaskName => "TaskGenerateFundingSummaryPeriodEndReport";

        private string ReportName => "Funding Summary Report";

        public FundingSummary(IExcelFileService excelFileService, IFileNameService fileNameService, IFundingSummaryDataProvider fundingSummaryDataProvider, IFundingSummaryModelBuilder fundingSummaryModelBuilder, IRenderService<FundingSummaryReportModel> fundingSummaryRenderService, IReportDataPersistanceService<FundingSummaryPersistModel> persistanceService, IFundingSummaryPersistanceMapper fundingSummaryPersistanceMapper)
        {
            _excelFileService = excelFileService;
            _fileNameService = fileNameService;
            _fundingSummaryDataProvider = fundingSummaryDataProvider;
            _fundingSummaryModelBuilder = fundingSummaryModelBuilder;
            _fundingSummaryRenderService = fundingSummaryRenderService;
            _persistanceService = persistanceService;
            _fundingSummaryPersistanceMapper = fundingSummaryPersistanceMapper;
        }

        public async Task<string> GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var fileName = _fileNameService.GetFilename(reportServiceContext, ReportName, OutputTypes.Excel, true);

            var fundingSummaryReferenceData = await _fundingSummaryDataProvider.ProvideAsync(reportServiceContext, cancellationToken);

            var model = _fundingSummaryModelBuilder.Build(reportServiceContext, fundingSummaryReferenceData);

            using (var workbook = _excelFileService.NewWorkbook())
            {
                workbook.Worksheets.Clear();

                var worksheet = _excelFileService.GetWorksheetFromWorkbook(workbook, "FundingSummaryReport");

                _fundingSummaryRenderService.Render(model, worksheet);

                await _excelFileService.SaveWorkbookAsync(workbook, fileName, reportServiceContext.Container, cancellationToken);
            }

            var persistModels = _fundingSummaryPersistanceMapper.Map(reportServiceContext, model, cancellationToken);
            await _persistanceService.PersistAsync(reportServiceContext, persistModels, cancellationToken);

            return fileName;
        }
    }
}
