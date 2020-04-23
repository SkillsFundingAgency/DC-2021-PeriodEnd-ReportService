using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Constants;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Interface;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Reports.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Reports.FundingSummaryReport.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Reports.FundingSummaryReport
{
    public class FundingSummaryReport : AbstractReport
    {
        private readonly IExcelService _excelService;
        private readonly IRenderService<IFundingSummaryReport> _fundingSummaryReportRenderService;
        private readonly IPeriodisedValuesLookupProviderService _periodisedValuesLookupProvider;
        private readonly IFCSProviderService _fcsProviderService;
        private readonly IFundingSummaryReportModelBuilder _modelBuilder;
        private readonly IPersistReportData _persistReportData;

        public FundingSummaryReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDateTimeProvider dateTimeProvider,
            IFundingSummaryReportModelBuilder modelBuilder,
            IExcelService excelService,
            IRenderService<IFundingSummaryReport> fundingSummaryReportRenderService,
            IPeriodisedValuesLookupProviderService periodisedValuesLookupProvider,
            IFCSProviderService fcsProviderService,
            IPersistReportData persistReportData)
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
            _persistReportData = persistReportData;
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

            var model = await _modelBuilder.BuildFundingSummaryReportModel(reportServiceContext, periodisedValuesLookup, fcsLookup, cancellationToken);

            using (var workbook = _excelService.NewWorkbook())
            {
                workbook.Worksheets.Clear();

                _fundingSummaryReportRenderService.Render(model, _excelService.GetWorksheetFromWorkbook(workbook, "Funding Summary"));

                var replacedFileName = $"{externalFileName}.xlsx".Replace('_', '/');

                _excelService.ApplyLicense();

                await _excelService.SaveWorkbookAsync(workbook, replacedFileName, reportServiceContext.Container, cancellationToken);

                await WriteZipEntry(archive, $"{fileName}.xlsx", workbook, cancellationToken);
            }

            if (reportServiceContext.DataPersistFeatureEnabled)
            {
                var persistModels = model.FundingCategories.SelectMany(fc => fc.FundingSubCategories.SelectMany(fsc =>
                    fsc.FundLineGroups.SelectMany(flg => flg.FundLines.Select(fl => new FundingSummaryPersistModel
                    {
                        Ukprn = reportServiceContext.Ukprn,
                        ContractNo = fc.ContractAllocationNumber,
                        FundingCategory = fc.FundingCategoryTitle,
                        FundingSubCategory = fsc.FundingSubCategoryTitle,
                        FundLine = fl.Title,
                        Aug19 = fl.Period1,
                        Sep19 = fl.Period2,
                        Oct19 = fl.Period3,
                        Nov19 = fl.Period4,
                        Dec19 = fl.Period5,
                        Jan20 = fl.Period6,
                        Feb20 = fl.Period7,
                        Mar20 = fl.Period8,
                        Apr20 = fl.Period9,
                        May20 = fl.Period10,
                        Jun20 = fl.Period11,
                        Jul20 = fl.Period12,
                        AugMar = fl.Period1To8,
                        AprJul = fl.Period9To12,
                        YearToDate = fl.YearToDate,
                        Total = fl.Total
                    })))).ToList();

                Stopwatch stopWatchLog = new Stopwatch();
                stopWatchLog.Start();
                await _persistReportData.PersistReportDataAsync(
                    persistModels,
                    reportServiceContext.Ukprn,
                    reportServiceContext.ReturnPeriod,
                    TableNameConstants.FundingSummaryReport,
                    reportServiceContext.ReportDataConnectionString,
                    cancellationToken);
                _logger.LogDebug($"Performance-FundingSummaryReport logging took - {stopWatchLog.ElapsedMilliseconds} ms ");
                stopWatchLog.Stop();
            }
            else
            {
                _logger.LogDebug(" Data Persist Feature is disabled.");
            }
        }
    }
}
