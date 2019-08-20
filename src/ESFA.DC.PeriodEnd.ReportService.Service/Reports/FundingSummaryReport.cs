//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using CsvHelper;
//using ESFA.DC.DateTimeProvider.Interface;
//using ESFA.DC.IO.Interfaces;
//using ESFA.DC.Logging.Interfaces;
//using ESFA.DC.PeriodEnd.ReportService.Interface;
//using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
//using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
//using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
//using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
//using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;
//using ESFA.DC.PeriodEnd.ReportService.Service.Mapper;
//using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract;

//namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports
//{
//    public class FundingSummaryReport : AbstractReport
//    {
//        //private readonly IFileNameService _fileNameService;
//        //private readonly IModelBuilder<IFundingSummaryReport> _fundingSummaryReportModelBuilder;
//        //private readonly IExcelService _excelService;
//        //private readonly IRenderService<IFundingSummaryReport> _fundingSummaryReportRenderService;

//        public FundingSummaryReport(
//            ILogger logger,
//            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
//            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
//            IFM36PeriodEndProviderService fm36ProviderService,
//            IDASPaymentsProviderService dasPaymentsProviderService,
//            ILarsProviderService larsProviderService,
//            IFcsProviderService fcsProviderService,
//            IDateTimeProvider dateTimeProvider,
//            IValueProvider valueProvider,
//            IAppsMonthlyPaymentModelBuilder modelBuilder)
//            : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService, logger)
//        {

//        }

//        public override string ReportFileName => "Funding Summary Report";

//        public override string ReportTaskName => ReportTaskNameConstants.FundingSummaryReport;

//        public override async Task GenerateReport(
//            IReportServiceContext reportServiceContext,
//            ZipArchive archive,
//            bool isFis,
//            CancellationToken cancellationToken)
//        {
//            var externalFileName = GetFilename(reportServiceContext);
//            var fileName = GetZipFilename(reportServiceContext);

//            // get the main base DAS payments report data
//            var appsMonthlyPaymentDasInfo = await _dasPaymentsProviderService.GetPaymentsInfoForAppsMonthlyPaymentReportAsync(reportServiceContext.Ukprn, cancellationToken);

//            // get the ILR data
//            var appsMonthlyPaymentIlrInfo = await _ilrPeriodEndProviderService.GetILRInfoForAppsMonthlyPaymentReportAsync(reportServiceContext.Ukprn, cancellationToken);

//            // Get the AEC data
//            var appsMonthlyPaymentRulebaseInfo = await _fm36ProviderService.GetFM36DataForAppsMonthlyPaymentReportAsync(reportServiceContext.Ukprn, cancellationToken);

//            // Get the Fcs Contract data
//            var appsMonthlyPaymentFcsInfo = await _fcsProviderService.GetFcsInfoForAppsMonthlyPaymentReportAsync(reportServiceContext.Ukprn, cancellationToken);

//            string[] learnAimRefs = appsMonthlyPaymentIlrInfo.Learners.SelectMany(x => x.LearningDeliveries).Select(x => x.LearnAimRef).Distinct().ToArray();

//            var appsMonthlyPaymentLarsLearningDeliveryInfos = await _larsProviderService.GetLarsLearningDeliveryInfoForAppsMonthlyPaymentReportAsync(learnAimRefs, cancellationToken);

//            var appsAdditionalPaymentsModel = _modelBuilder.BuildAppsMonthlyPaymentModelList(appsMonthlyPaymentIlrInfo, appsMonthlyPaymentRulebaseInfo, appsMonthlyPaymentDasInfo, appsMonthlyPaymentFcsInfo, appsMonthlyPaymentLarsLearningDeliveryInfos);

//            string csv = await GetCsv(appsAdditionalPaymentsModel, cancellationToken);
//            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
//            await WriteZipEntry(archive, $"{fileName}.csv", csv);
//        }

//        private async Task<string> GetCsv(IReadOnlyList<AppsMonthlyPaymentModel> appsAdditionalPaymentsModel, CancellationToken cancellationToken)
//        {
//            cancellationToken.ThrowIfCancellationRequested();

//            using (MemoryStream ms = new MemoryStream())
//            {
//                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
//                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
//                {
//                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
//                    {
//                        WriteCsvRecords<AppsMonthlyPaymentMapper, AppsMonthlyPaymentModel>(csvWriter, appsAdditionalPaymentsModel);

//                        csvWriter.Flush();
//                        textWriter.Flush();
//                        return Encoding.UTF8.GetString(ms.ToArray());
//                    }
//                }
//            }
//        }
//    }
//}
