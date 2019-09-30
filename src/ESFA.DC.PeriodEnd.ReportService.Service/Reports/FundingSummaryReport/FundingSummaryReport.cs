using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
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




        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;

        private readonly IFm35PeriodEndProviderService _fm35ProviderService;


        private readonly ILarsProviderService _larsProviderService;

        private readonly IAppsMonthlyPaymentModelBuilder _modelBuilder;

        private readonly IFileNameService _fileNameService;
        private readonly IModelBuilder<IFundingSummaryReport> _fundingSummaryReportModelBuilder;
        private readonly IExcelService _excelService;
        private readonly IRenderService<IFundingSummaryReport> _fundingSummaryReportRenderService;
        #endregion Member variable initialisation

        #region Constructors
        public FundingSummaryReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDASPaymentsProviderService dasPaymentsProviderService,
            IlrRulebaseProviderService ilrRulebaseProviderService,
            IFm35PeriodEndProviderService fm35ProviderService,
            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
            IFM36PeriodEndProviderService fm36ProviderService,
            ILarsProviderService larsProviderService,
            IFCSProviderService fcsProviderService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            IAppsMonthlyPaymentModelBuilder modelBuilder)

        //IFileNameService fileNameService,
        //IModelBuilder<IFundingSummaryReport> fundingSummaryReportModelBuilder,
        //IExcelService excelService,
        //IRenderService<IFundingSummaryReport> fundingSummaryReportRenderService)
        : base(
            dateTimeProvider,
            valueProvider,
            streamableKeyValuePersistenceService,
            logger)
        {
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _fm35ProviderService = ilrRulebaseProviderService;
            _fm36ProviderService = fm36ProviderService;
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _larsProviderService = larsProviderService;
            _fcsProviderService = fcsProviderService;
            _modelBuilder = modelBuilder;
//            _fileNameService = fileNameService;
//            _fundingSummaryReportModelBuilder = fundingSummaryReportModelBuilder;
//            _excelService = excelService;
//            _fundingSummaryReportRenderService = fundingSummaryReportRenderService;
        }
        #endregion Constructors

        #region Base class property initialisation
        public override string ReportFileName => "Funding Summary Report";

        public override string ReportTaskName => ReportTaskNameConstants.AppsMonthlyPaymentReport;
        #endregion Base class property initialisation

        // Base class method overrides/implementation
        public override void CsvWriterConfiguration(CsvWriter csvWriter)
        {
            csvWriter.Configuration.TypeConverterOptionsCache.GetOptions(typeof(decimal?)).Formats = new[] { "############0.00" };
        }

        public override async Task GenerateReport(
             IReportServiceContext reportServiceContext,
             ZipArchive archive,
             bool isFis,
             CancellationToken cancellationToken)
        {
            var externalFileName = GetFilename(reportServiceContext);
            var fileName = GetZipFilename(reportServiceContext);

            // get the DAS payments data
            var fm35LearningDeliveryPeriodisedValues = _fm35ProviderService.GetFM35LearningDeliveryPerioisedValues(reportServiceContext.Ukprn);

            // get the EAS data
            var providerEasInfo =
                await _easProviderService.GetProviderEasInfoForFundingSummaryReport(reportServiceContext.Ukprn, cancellationToken);

            // get the ILR data
            var appsMonthlyPaymentIlrInfo =
                await _ilrPeriodEndProviderService.GetILRInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn, cancellationToken);

            // Get the Fcs Contract data
            var appsMonthlyPaymentFcsInfo =
                await _fcsProviderService.GetFcsInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn,
                    cancellationToken);

            // Get the name's of the learning aims
            string[] learnAimRefs = appsMonthlyPaymentIlrInfo.Learners.SelectMany(x => x.LearningDeliveries)
                .Select(x => x.LearnAimRef).Distinct().ToArray();
            var appsMonthlyPaymentLarsLearningDeliveryInfos =
                await _larsProviderService.GetLarsLearningDeliveryInfoForAppsMonthlyPaymentReportAsync(
                    learnAimRefs,
                    cancellationToken);

            // Build the Report
            var appsMonthlyPaymentsModel = _modelBuilder.BuildAppsMonthlyPaymentModelList(
                appsMonthlyPaymentIlrInfo,
                appsMonthlyPaymentRulebaseInfo,
                appsMonthlyPaymentDasInfo,
                appsMonthlyPaymentDasEarningsInfo,
                appsMonthlyPaymentFcsInfo,
                appsMonthlyPaymentLarsLearningDeliveryInfos);

            //var fundingSummaryReportModel = _fundingSummaryReportModelBuilder.Build(reportServiceContext, reportsDependentData);

            string csv = await GetCsv(appsMonthlyPaymentsModel, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private async Task<string> GetCsv(IReadOnlyList<AppsMonthlyPaymentModel> appsMonthlyPaymentsModel, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<AppsMonthlyPaymentMapper, AppsMonthlyPaymentModel>(csvWriter, appsMonthlyPaymentsModel);

                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
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
