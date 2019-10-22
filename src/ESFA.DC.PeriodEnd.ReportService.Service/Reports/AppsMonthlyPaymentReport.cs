using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
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
        private readonly IPersistReportData _persistReportData;

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
            IAppsMonthlyPaymentModelBuilder modelBuilder,
            IPersistReportData persistReportData)
        : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService, logger)
        {
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _fm36ProviderService = fm36ProviderService;
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _larsProviderService = larsProviderService;
            _fcsProviderService = fcsProviderService;
            _modelBuilder = modelBuilder;
            _persistReportData = persistReportData;
        }

        public override string ReportFileName => "Apps Monthly Payment Report";

        public override string ReportTaskName => ReportTaskNameConstants.AppsMonthlyPaymentReport;

        public override void ApplyConfiguration(CsvWriter csvWriter)
        {
            csvWriter.Configuration.TypeConverterOptionsCache.GetOptions(typeof(decimal?)).Formats = new[] { "############0.00000" };
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

            var connectionString = reportServiceContext.ReportDataConnectionString;
            //var connectionString = "data source=(local);initial catalog=ESFA.DC.PeriodEnd.ReportDatabase1920;user id=sa;password=zyxel123$;Connect Timeout=90";
            await _persistReportData.PersistAppsAdditionalPaymentAsync(
                                    (List<AppsMonthlyPaymentModel>)appsMonthlyPaymentsModel,
                                    reportServiceContext.Ukprn,
                                    reportServiceContext.ReturnPeriod,
                                    connectionString,
                                    cancellationToken);
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

        //private async Task PersistAppsMonthlyPaymentReport(
        //    IReportServiceContext reportServiceContext,
        //    CancellationToken cancellationToken,
        //    IReadOnlyList<AppsMonthlyPaymentModel> appsMonthlyPaymentsModel)
        //{
        //    using (SqlConnection sqlConnection =
        //        new SqlConnection(
        //            "data source=(local);initial catalog=ESFA.DC.PeriodEnd.ReportDatabase1920;user id=sa;password=zyxel123$;Connect Timeout=90"))
        //    {
        //        await sqlConnection.OpenAsync(cancellationToken);
        //        cancellationToken.ThrowIfCancellationRequested();
        //        _logger.LogDebug("Starting Persisting Apps Monthly Payment Report Data");

        //        using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
        //        {
        //            try
        //            {
        //                await _persistReportData.PersistAppsAdditionalPaymentAsync(
        //                    (List<AppsMonthlyPaymentModel>)appsMonthlyPaymentsModel,
        //                    reportServiceContext.Ukprn,
        //                    reportServiceContext.ReturnPeriod,
        //                    sqlConnection,
        //                    sqlTransaction,
        //                    cancellationToken);
        //                sqlTransaction.Commit();
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogDebug(
        //                    $"Persisting Apps Monthly Payment Report Data failed attempting to rollback - {ex.Message}");
        //                sqlTransaction.Rollback();
        //                _logger.LogDebug(" Persisting Apps Monthly Payment Report Data successfully rolled back");

        //                throw;
        //            }
        //        }
        //    }
        //}
    }
}
