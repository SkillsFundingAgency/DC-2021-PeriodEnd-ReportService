﻿using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Mapper;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport
{
    public class FundingSummaryReport : AbstractReport
    {
        #region Member variable initialisation

        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IlrRulebaseProviderService _ilrRulebaseProviderService;
        private readonly IEasProviderService _easProviderService;
        private readonly IFCSProviderService _fcsProviderService;

        private readonly IFileNameService _fileNameService;
        private readonly IExcelService _excelService;
        private readonly IRenderService<IFundingSummaryReport> _fundingSummaryReportRenderService;
        private readonly IFundingSummaryReportModelBuilder _modelBuilder;

        #endregion Member variable initialisation

        #region Constructors

        public FundingSummaryReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDASPaymentsProviderService dasPaymentsProviderService,
            IlrRulebaseProviderService ilrRulebaseProviderService,
            IEasProviderService easProviderService,
            IFCSProviderService fcsProviderService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            // IAppsMonthlyPaymentModelBuilder modelBuilder)
            IFundingSummaryReportModelBuilder modelBuilder,

            //IFileNameService fileNameService,
            //IModelBuilder<IFundingSummaryReport> fundingSummaryReportModelBuilder,
            IExcelService excelService,
            IRenderService<IFundingSummaryReport> fundingSummaryReportRenderService)
            : base(
                dateTimeProvider,
                valueProvider,
                streamableKeyValuePersistenceService,
                logger)
        {
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _ilrRulebaseProviderService = ilrRulebaseProviderService;
            _easProviderService = easProviderService;
            _fcsProviderService = fcsProviderService;
            _modelBuilder = modelBuilder;
//            _fileNameService = fileNameService;
//            _fundingSummaryReportModelBuilder = fundingSummaryReportModelBuilder;
            _excelService = excelService;
            _fundingSummaryReportRenderService = fundingSummaryReportRenderService;
        }

        #endregion Constructors

        #region Base class property initialisation

        public override string ReportFileName => "Funding Summary Report";

        public override string ReportTaskName => ReportTaskNameConstants.AppsMonthlyPaymentReport;

        #endregion Base class property initialisation

        // Base class method overrides/implementation
        public override void CsvWriterConfiguration(CsvWriter csvWriter)
        {
            csvWriter.Configuration.TypeConverterOptionsCache.GetOptions(typeof(decimal?)).Formats =
                new[] {"############0.00"};
        }

        public override async Task GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            bool isFis,
            CancellationToken cancellationToken)
        {
            var externalFileName = GetFilename(reportServiceContext);
            var fileName = GetZipFilename(reportServiceContext);
            var fundingSummaryReportData = GetFundingSummaryReportData(reportServiceContext);

            // get the DAS payments data
            var fm35LearningDeliveryPeriodisedValues =
                _ilrRulebaseProviderService.GetFm35LearningDeliveryPeriodisedValues(reportServiceContext.Ukprn);

            // get the EAS data
            var providerEasInfo =
                await _easProviderService.GetProviderEasInfoForFundingSummaryReport(reportServiceContext.Ukprn,
                    cancellationToken);

            // Get the Fcs Contract data
            var appsMonthlyPaymentFcsInfo =
                await _fcsProviderService.GetFcsInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn,
                    cancellationToken);

            // Build the Report
            //var fundingSummaryReportModel = _modelBuilder.BuildFundingSummaryReportModel(
            //    fm35LearningDeliveryPeriodisedValues,
            //    providerEasInfo,
            //    appsMonthlyPaymentFcsInfo);

            //using (var workbook = _excelService.NewWorkbook())
            //{
            //    var worksheet = _excelService.GetWorksheetFromWorkbook(workbook, 0);

            //    _fundingSummaryReportRenderService.Render(fundingSummaryReportModel, worksheet);

            //    await _excelService.SaveWorkbookAsync(workbook, fileName, reportServiceContext.Container, cancellationToken);
            //}

            //var fundingSummaryReportModel = _fundingSummaryReportModelBuilder.Build(reportServiceContext, reportsDependentData);

            //string csv = await GetCsv(fundingSummaryReportModel, cancellationToken);
            //await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            //await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private async Task<string> GetCsv(IReadOnlyList<AppsMonthlyPaymentModel> appsMonthlyPaymentsModel,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<AppsMonthlyPaymentMapper, AppsMonthlyPaymentModel>(csvWriter,
                            appsMonthlyPaymentsModel);

                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        private FundingSummaryReportData GetFundingSummaryReportData(IReportServiceContext reportServiceContext)
        {
            FundingSummaryReportData fundingSummaryReportData = new FundingSummaryReportData();

            var fm25LearnerPeriodisedValues = _ilrRulebaseProviderService.GetFm25LearnerPeriodisedValues(reportServiceContext.Ukprn);

            var fm35LearningDeliveryPeriodisedValues = _ilrRulebaseProviderService.GetFm35LearningDeliveryPeriodisedValues(reportServiceContext.Ukprn);

            return fundingSummaryReportData;
        }
    }

    //public class FundingSummaryReport : BaseReport, IBaseReport
    //{
    //    private readonly IFileNameService _fileNameService;
    //    private readonly IModelBuilder<IFundingSummaryReport> _fundingSummaryReportModelBuilder;
    //    private readonly IExcelService _excelService;
    //    private readonly IRenderService<IFundingSummaryReport> _fundingSummaryReportRenderService;

    //    public FundingSummaryReport(
    //        IFileNameService fileNameService,
    //        IModelBuilder<IFundingSummaryReport> fundingSummaryReportModelBuilder,
    //        IExcelService excelService,
    //        IRenderService<IFundingSummaryReport> fundingSummaryReportRenderService)
    //        : base(ReportTaskNameConstants.FundingSummaryReport, "Funding Summary Report")
    //    {
    //        _fileNameService = fileNameService;
    //        _fundingSummaryReportModelBuilder = fundingSummaryReportModelBuilder;
    //        _excelService = excelService;
    //        _fundingSummaryReportRenderService = fundingSummaryReportRenderService;
    //    }

    //    public virtual IEnumerable<Type> DependsOn
    //        => new[]
    //        {
    //            DependentDataCatalog.Fm25,
    //            DependentDataCatalog.Fm35,
    //            DependentDataCatalog.Fm36,
    //            DependentDataCatalog.Fm81,
    //            DependentDataCatalog.Fm99,
    //            DependentDataCatalog.ReferenceData,
    //        };

    //    public async Task<IEnumerable<string>> GenerateAsync(
    //        IReportServiceContext reportServiceContext,
    //        IReportServiceDependentData reportsDependentData,
    //        CancellationToken cancellationToken)
    //    {
    //        var fundingSummaryReportModel = _fundingSummaryReportModelBuilder.Build(reportServiceContext, reportsDependentData);

    //        var fileName = _fileNameService.GetFilename(reportServiceContext, FileName, OutputTypes.Excel);

    //        using (var workbook = _excelService.NewWorkbook())
    //        {
    //            var worksheet = _excelService.GetWorksheetFromWorkbook(workbook, 0);

    //            _fundingSummaryReportRenderService.Render(fundingSummaryReportModel, worksheet);

    //            await _excelService.SaveWorkbookAsync(workbook, fileName, reportServiceContext.Container, cancellationToken);
    //        }

    //        return new[] { fileName };
    //    }
    //}
}
