﻿using System;
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
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.ReferenceData.Organisations.Model;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public sealed class ProviderSubmissionsReport : AbstractInternalReport, IInternalReport
    {
        private const string TemplateName = "ProviderSubmissionsReportTemplate.xlsx";
        private const string ProviderSubmissionTabName = "ILR Provider Submissions Report";

        private readonly ILogger _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IOrgProviderService _orgProviderService;
        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;
        private readonly IProviderSubmissionsModelBuilder _providerSubmissionsModelBuilder;
        private readonly IJobQueueManagerProviderService _jobQueueManagerProviderService;
        private readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;

        public ProviderSubmissionsReport(
            ILogger logger,
            IDateTimeProvider dateTimeProvider,
            IOrgProviderService orgProviderService,
            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
            IProviderSubmissionsModelBuilder providerSubmissionsModelBuilder,
            IJobQueueManagerProviderService jobQueueManagerProviderService,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IValueProvider valueProvider)
            : base(valueProvider, dateTimeProvider)
        {
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _orgProviderService = orgProviderService;
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _providerSubmissionsModelBuilder = providerSubmissionsModelBuilder;
            _jobQueueManagerProviderService = jobQueueManagerProviderService;
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
        }

        public override string ReportFileName { get; set; } = "ILR Provider Submissions Report";

        public override string ReportTaskName => ReportTaskNameConstants.InternalReports.ProviderSubmissionsReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            _logger.LogInfo($"In {ReportFileName} report.");
            List<long> ukprns = new List<long>();

            var externalFileName = GetFilename(reportServiceContext);

            IEnumerable<FileDetail> fileDetails = await _ilrPeriodEndProviderService
                .GetFileDetailsLatestSubmittedAsync(cancellationToken);

            IEnumerable<long> ukPrns = fileDetails.Select(x => (long)x.UKPRN).Distinct();
            IEnumerable<OrgDetail> orgDetails = await _orgProviderService.GetOrgDetailsForUKPRNsAsync(ukPrns.ToList(), cancellationToken);

            IEnumerable<long> expectedReturners = await _jobQueueManagerProviderService
                .GetExpectedReturnersUKPRNsAsync(
                    reportServiceContext.CollectionName,
                    reportServiceContext.ReturnPeriod,
                    reportServiceContext.ILRPeriods,
                    cancellationToken);

            IEnumerable<long> actualReturners = await _jobQueueManagerProviderService
                .GetActualReturnersUKPRNsAsync(
                    reportServiceContext.CollectionName,
                    reportServiceContext.ReturnPeriod,
                    reportServiceContext.ILRPeriods,
                    cancellationToken);

            IEnumerable<ProviderSubmissionModel> providerSubmissionsModel = _providerSubmissionsModelBuilder
                .BuildModel(fileDetails, orgDetails, expectedReturners, actualReturners, reportServiceContext.ILRPeriods);

            Workbook providerSubmissionWorkbook = GenerateWorkbook(
                reportServiceContext.ReturnPeriod,
                providerSubmissionsModel);

            using (var ms = new MemoryStream())
            {
                providerSubmissionWorkbook.Save(ms, SaveFormat.Xlsx);
                await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.xlsx", ms, cancellationToken);
            }
        }

        public Workbook GenerateWorkbook(int periodNumber, IEnumerable<ProviderSubmissionModel> providerSubmissionModels)
        {
            Workbook workbook = GetWorkbookFromTemplate(TemplateName);
            var worksheet = workbook.Worksheets[ProviderSubmissionTabName];

            var returned = providerSubmissionModels.Count(x => x.Returned);
            var expectedButNotReturned = providerSubmissionModels.Count(x => x.Expected && !x.Returned);
            var zeroLearnerReturns = providerSubmissionModels.Count(x => x.Returned && x.TotalValid == 0);
            var expected = providerSubmissionModels.Count(x => x.Expected);
            var returningExpected = providerSubmissionModels.Count(x => x.Returned && x.Expected);
            var returningUnexpected = providerSubmissionModels.Count(x => x.Returned && !x.Expected);

            worksheet.Cells[1, 1].PutValue($"ILR Provider Submissions Report - R{periodNumber:D2}");
            worksheet.Cells[2, 1].PutValue($"Report Run: {_dateTimeProvider.ConvertUtcToUk(_dateTimeProvider.GetNowUtc()).ToString("u")}");
            worksheet.Cells[4, 1].PutValue($"Total No of Returning Providers: {returned}");
            worksheet.Cells[5, 1].PutValue($"No of Expected Providers Not Returning: {expectedButNotReturned}");
            worksheet.Cells[6, 1].PutValue($"No of \"0 Learner\" Returns: {zeroLearnerReturns}");
            worksheet.Cells[7, 1].PutValue($"No of Providers Expected to Return: {expected}");
            worksheet.Cells[8, 1].PutValue($"No of Returning Expected Providers: {returningExpected}");
            worksheet.Cells[9, 1].PutValue($"No of Returning Unexpected Providers: {returningUnexpected}");

            var designer = new WorkbookDesigner
            {
                Workbook = workbook
            };

            designer.SetDataSource("ProviderSubmissions", providerSubmissionModels);
            designer.Process();

            return workbook;
        }
    }
}