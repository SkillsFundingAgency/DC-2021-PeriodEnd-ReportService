using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport
{
    public class FundingSummaryReport : AbstractReport
    {
        private readonly IExcelService _excelService;
        private readonly IRenderService<IFundingSummaryReport> _fundingSummaryReportRenderService;
        private readonly IPeriodisedValuesLookupProviderService _periodisedValuesLookupProvider;
        private readonly IFCSProviderService _fcsProviderService;
        private readonly IFundingSummaryReportModelBuilder _modelBuilder;

        public FundingSummaryReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDateTimeProvider dateTimeProvider,
            IFundingSummaryReportModelBuilder modelBuilder,
            IExcelService excelService,
            IRenderService<IFundingSummaryReport> fundingSummaryReportRenderService,
            IPeriodisedValuesLookupProviderService periodisedValuesLookupProvider,
            IFCSProviderService fcsProviderService)
            : base(
                dateTimeProvider,
                streamableKeyValuePersistenceService,
                logger)
        {
            _modelBuilder = modelBuilder;
            _excelService = excelService;
            _fundingSummaryReportRenderService = fundingSummaryReportRenderService;
            _periodisedValuesLookupProvider = periodisedValuesLookupProvider;
            _fcsProviderService = fcsProviderService;
        }

        public override string ReportFileName => "Funding Summary Report";

        public override string ReportTaskName => ReportTaskNameConstants.FundingSummaryReport;

        public override async Task GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            CancellationToken cancellationToken)
        {
            var externalFileName = GetFilename(reportServiceContext);
            var fileName = GetZipFilename(reportServiceContext);

            var fcsLookup = await _fcsProviderService.GetContractAllocationNumberFSPCodeLookupAsync(reportServiceContext.Ukprn, cancellationToken);
            var periodisedValuesLookup = await _periodisedValuesLookupProvider.ProvideAsync(reportServiceContext, cancellationToken);

            var model = _modelBuilder.BuildFundingSummaryReportModel(reportServiceContext, periodisedValuesLookup, fcsLookup);

            using (var workbook = _excelService.NewWorkbook())
            {
                workbook.Worksheets.Clear();

                _fundingSummaryReportRenderService.Render(model, _excelService.GetWorksheetFromWorkbook(workbook, "Funding Summary"));

                await _excelService.SaveWorkbookAsync(workbook, $"{externalFileName}.xlsx", reportServiceContext.Container, cancellationToken);

                //await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.xlsx", workbook.SaveToStream(), cancellationToken);
                //await WriteZipEntry(archive, $"{fileName}.csv", csv);
            }
        }
    }
}
