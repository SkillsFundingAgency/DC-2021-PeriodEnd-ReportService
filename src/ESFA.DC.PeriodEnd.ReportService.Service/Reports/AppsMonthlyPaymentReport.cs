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
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Service.Mapper;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports
{
    public class AppsMonthlyPaymentReport : AbstractReport
    {
        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;
        private readonly IFM36PeriodEndProviderService _fm36ProviderService;
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IDASPaymentsProviderService _dasEarningsProviderService;
        private readonly ILarsProviderService _larsProviderService;
        private readonly IFCSProviderService _fcsProviderService;
        private readonly IAppsMonthlyPaymentModelBuilder _modelBuilder;

        public AppsMonthlyPaymentReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
            IFM36PeriodEndProviderService fm36ProviderService,
            IDASPaymentsProviderService dasPaymentsProviderService,
            ILarsProviderService larsProviderService,
            IFCSProviderService fcsProviderService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            IAppsMonthlyPaymentModelBuilder modelBuilder)
        : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService, logger)
        {
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _fm36ProviderService = fm36ProviderService;
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _larsProviderService = larsProviderService;
            _fcsProviderService = fcsProviderService;
            _modelBuilder = modelBuilder;
        }

        public override string ReportFileName => "Apps Monthly Payment Report";

        public override string ReportTaskName => ReportTaskNameConstants.AppsMonthlyPaymentReport;

        public override void ApplyConfiguration(CsvWriter csvWriter)
        {
            csvWriter.Configuration.TypeConverterOptionsCache.GetOptions(typeof(decimal?)).Formats =
                new[] { "#############.##" };
        }

        public override async Task GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            bool isFis,
            CancellationToken cancellationToken)
        {
            var externalFileName = GetFilename(reportServiceContext);
            var fileName = GetZipFilename(reportServiceContext);

            // get the main base DAS payments data
            var appsMonthlyPaymentDasInfo =
                await _dasPaymentsProviderService.GetPaymentsInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn, cancellationToken);

            // get the DAS Earnings Event data
            var appsMonthlyPaymentDasEarningsInfo =
                await _dasPaymentsProviderService.GetEarningsInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn, cancellationToken);

            // get the ILR data
            var appsMonthlyPaymentIlrInfo =
                await _ilrPeriodEndProviderService.GetILRInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn, cancellationToken);

            // Get the AEC data
            var appsMonthlyPaymentRulebaseInfo =
                await _fm36ProviderService.GetRulebaseDataForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn,
                    cancellationToken);

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

            // Build the actual Apps Monthly Payment Report
            var appsMonthlyPaymentsModel = _modelBuilder.BuildAppsMonthlyPaymentModelList(
                appsMonthlyPaymentIlrInfo,
                appsMonthlyPaymentRulebaseInfo,
                appsMonthlyPaymentDasInfo,
                appsMonthlyPaymentDasEarningsInfo,
                appsMonthlyPaymentFcsInfo,
                appsMonthlyPaymentLarsLearningDeliveryInfos);

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
}
