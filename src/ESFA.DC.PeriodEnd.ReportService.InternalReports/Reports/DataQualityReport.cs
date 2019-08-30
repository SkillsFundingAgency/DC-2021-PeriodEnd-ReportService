using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Mappers;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataExtractReport;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public class DataQualityReport : AbstractInternalReport, IInternalReport
    {
        private const string TemplateName = "ILRDataQualityReportTemplate.xlsx";
        private const string DataQualityTabName = "ILR Data Quality Reports";

        private readonly ILogger _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IReportServiceContext _reportServiceContext;
        private readonly IJobQueueDataProviderService _jobQueueDataProviderService;
        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;
        private readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;

        public DataQualityReport(
            ILogger logger,
            IDateTimeProvider dateTimeProvider,
            IReportServiceContext reportServiceContext,
            IJobQueueDataProviderService jobQueueDataProviderService,
            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IValueProvider valueProvider)
        : base(valueProvider, dateTimeProvider)
        {
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _reportServiceContext = reportServiceContext;
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _jobQueueDataProviderService = jobQueueDataProviderService;
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
        }

        public override string ReportFileName { get; set; } = "Data Quality Report";

        public override string ReportTaskName => ReportTaskNameConstants.InternalReports.DataQualityReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            _logger.LogInfo($"In {ReportFileName} report.");

            var externalFileName = GetFilename(reportServiceContext);

            List<ReturnPeriod> returnPeriods = (await _jobQueueDataProviderService.GetReturnPeriodsAsync(_reportServiceContext.CollectionYear, cancellationToken)).ToList();
            IEnumerable<DataQualityReturningProviders> dataQualityModels = await _ilrPeriodEndProviderService.GetReturningProvidersAsync(
                _reportServiceContext.CollectionYear,
                returnPeriods,
                CancellationToken.None);

            IEnumerable<RuleViolationsInfo> ruleViolations = await _ilrPeriodEndProviderService.GetTop20RuleViolationsAsync(CancellationToken.None);

            IEnumerable<ProviderWithoutValidLearners> providersWithoutValidLearners = null; //await ProvidersWithoutValidLearners(CancellationToken.None);

            IEnumerable<Top10ProvidersWithInvalidLearners> providersWithInvalidLearners = null; // await ProvidersWithInvalidLearners(_reportServiceContext.CollectionYear, CancellationToken.None);

            Workbook dataQualityWorkbook = GenerateWorkbook(
                _reportServiceContext.ReturnPeriod,
                dataQualityModels,
                ruleViolations,
                providersWithoutValidLearners,
                providersWithInvalidLearners);

            using (var ms = new MemoryStream())
            {
                dataQualityWorkbook.Save(ms, SaveFormat.Xlsx);
                await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.xlsx", ms, cancellationToken);
            }
        }

        public Workbook GenerateWorkbook(
            int periodNumber,
            IEnumerable<DataQualityReturningProviders> returningProviderModels,
            IEnumerable<RuleViolationsInfo> ruleViolationsInfoModels,
            IEnumerable<ProviderWithoutValidLearners> providersWithoutValidLearners,
            IEnumerable<Top10ProvidersWithInvalidLearners> top10ProvidersWithInvalidLearners)
        {
            Workbook workbook = GetWorkbookFromTemplate(TemplateName);
            var worksheet = workbook.Worksheets[DataQualityTabName];

            worksheet.Cells[1, 1].PutValue($"ILR Data Quality Reports - R{periodNumber.ToString().PadLeft(2, '0')}");
            worksheet.Cells[2, 1].PutValue($"Report Run: {_dateTimeProvider.ConvertUtcToUk(_dateTimeProvider.GetNowUtc()).ToString("u")}");

            var designer = new WorkbookDesigner
            {
                Workbook = workbook
            };

            designer.SetDataSource("ReturningProvidersInfo", returningProviderModels);
            designer.SetDataSource("RuleViolationsInfo", ruleViolationsInfoModels);
            designer.SetDataSource("ProviderWithoutValidLearnerInfo", providersWithoutValidLearners);
            designer.SetDataSource("Top10ProvidersWithInvalidLearners", top10ProvidersWithInvalidLearners);
            designer.Process();

            return workbook;
        }

        private string GetLatestReturn(DateTime submittedDateTime, List<ReturnPeriod> returnPeriods)
        {
            int returnPeriod = returnPeriods
                                   .SingleOrDefault(x =>
                                   x.StartDateTimeUtc >= submittedDateTime
                                   && x.EndDateTimeUtc <= submittedDateTime)
                                        ?.PeriodNumber ?? 0;

            return $"R{returnPeriod.ToString().PadLeft(2, '0')}";
        }
    }
}
