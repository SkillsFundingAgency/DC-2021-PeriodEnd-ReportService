using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR1920.DataStore.EF;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.ReferenceData.Organisations.Model;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public class DataQualityReport : AbstractInternalReport, IInternalReport
    {
        private const string TemplateName = "ILRDataQualityReportTemplate.xlsx";
        private const string DataQualityTabName = "Data Quality";

        private readonly ILogger _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IOrgProviderService _orgProviderService;
        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;
        private readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;

        public DataQualityReport(
            ILogger logger,
            IDateTimeProvider dateTimeProvider,
            IOrgProviderService orgProviderService,
            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IValueProvider valueProvider)
        : base(valueProvider, dateTimeProvider)
        {
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _orgProviderService = orgProviderService;
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
        }

        public override string ReportFileName { get; set; } = "Data Quality Report";

        public override string ReportTaskName => ReportTaskNameConstants.InternalReports.DataQualityReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            _logger.LogInfo($"In {ReportFileName} report.");
            List<long> ukprns = new List<long>();

            var externalFileName = GetFilename(reportServiceContext);
            List<FileDetail> fileDetails = (await _ilrPeriodEndProviderService.GetFileDetailsAsync(CancellationToken.None)).ToList();

            IEnumerable<DataQualityReturningProviders> dataQualityModels = await _ilrPeriodEndProviderService.GetReturningProvidersAsync(
                reportServiceContext.CollectionYear,
                reportServiceContext.ILRPeriods,
                fileDetails,
                CancellationToken.None);

            IEnumerable<RuleViolationsInfo> ruleViolations = await _ilrPeriodEndProviderService.GetTop20RuleViolationsAsync(CancellationToken.None);

            IEnumerable<ProviderWithoutValidLearners> providersWithoutValidLearners = (await
                _ilrPeriodEndProviderService.GetProvidersWithoutValidLearners(fileDetails, CancellationToken.None)).ToList();
            ukprns.AddRange(providersWithoutValidLearners.Select(x => (long)x.Ukprn));

            IEnumerable<Top10ProvidersWithInvalidLearners> providersWithInvalidLearners = (await
                _ilrPeriodEndProviderService.GetProvidersWithInvalidLearners(
                reportServiceContext.CollectionYear,
                reportServiceContext.ILRPeriods,
                fileDetails,
                CancellationToken.None)).ToList();
            ukprns.AddRange(providersWithInvalidLearners.Select(x => x.Ukprn));

            IEnumerable<OrgDetail> orgDetails = await _orgProviderService.GetOrgDetailsForUKPRNsAsync(ukprns.Distinct().ToList(), CancellationToken.None);
            foreach (var org in orgDetails)
            {
                var valid = providersWithoutValidLearners.SingleOrDefault(p => p.Ukprn == org.Ukprn);
                if (valid != null)
                {
                    valid.Name = org.Name;
                }

                var invalid = providersWithInvalidLearners.SingleOrDefault(p => p.Ukprn == org.Ukprn);
                if (invalid != null)
                {
                    invalid.Name = org.Name;
                    invalid.Status = org.Status;
                }
            }

            Workbook dataQualityWorkbook = GenerateWorkbook(
                reportServiceContext.ReturnPeriod,
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

            worksheet.Cells[1, 1].PutValue($"ILR Data Quality Reports - R{periodNumber:D2}");
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
    }
}
