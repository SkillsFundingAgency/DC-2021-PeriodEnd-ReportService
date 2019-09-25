using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public sealed class CollectionStatsReport : AbstractInternalReport
    {
        private readonly Func<IJobQueueDataContext> _jobQueueDataFactory;

        public CollectionStatsReport(
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            Func<IJobQueueDataContext> jobQueueDataFactory)
            : base(valueProvider, dateTimeProvider)
        {
            _jobQueueDataFactory = jobQueueDataFactory;
        }

        public override string ReportFileName { get; set; } = "Collection Stats Report";

        public override string ReportTaskName => ReportTaskNameConstants.InternalReports.CollectionStatsReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            //var externalFileName = GetFilename(reportServiceContext);

            //IEnumerable<DataExtractModel> summarisationInfo = (await _summarisationProviderService.GetSummarisedActualsForDataExtractReport(
            //    new[] { reportServiceContext.CollectionReturnCodeApp, reportServiceContext.CollectionReturnCodeDC, reportServiceContext.CollectionReturnCodeESF },
            //    cancellationToken)).ToList();
            //IEnumerable<string> organisationIds = summarisationInfo?.Select(x => x.OrganisationId).Distinct();
            //IEnumerable<DataExtractFcsInfo> fcsInfo = await _fcsProviderService.GetFCSForDataExtractReport(organisationIds, cancellationToken);

            //var dataExtractModel = _modelBuilder.BuildModel(summarisationInfo, fcsInfo);
            //string csv = await GetCsv(dataExtractModel, cancellationToken);
            //await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
        }

        private async Task<string> GetCsv(
            IEnumerable<DataExtractModel> dataExtractModel,
            CancellationToken cancellationToken)
        {
            return null;
            //cancellationToken.ThrowIfCancellationRequested();

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
            //    using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
            //    {
            //        using (CsvWriter csvWriter = new CsvWriter(textWriter))
            //        {
            //            WriteCsvRecords<DataExtractMapper, DataExtractModel>(csvWriter, dataExtractModel);

            //            csvWriter.Flush();
            //            textWriter.Flush();
            //            return Encoding.UTF8.GetString(ms.ToArray());
            //        }
            //    }
            //}
        }
    }
}
