using System.Collections.Generic;
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
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Mappers;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataExtractReport;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public class DataExtractReport : AbstractInternalReport, IInternalReport
    {
        private readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;
        private readonly ISummarisationProviderService _summarisationProviderService;
        private readonly IFCSProviderService _fcsProviderService;
        private readonly IDataExtractModelBuilder _modelBuilder;

        public DataExtractReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            ISummarisationProviderService summarisationProviderService,
            IFCSProviderService fcsProviderService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            IDataExtractModelBuilder modelBuilder)
        : base(valueProvider, dateTimeProvider)
        {
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
            _summarisationProviderService = summarisationProviderService;
            _fcsProviderService = fcsProviderService;
            _modelBuilder = modelBuilder;
        }

        public override string ReportFileName { get; set; } = "Data Extract Report";

        public override string ReportTaskName => ReportTaskNameConstants.InternalReports.DataExtractReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var externalFileName = GetFilename(reportServiceContext);

            IEnumerable<DataExtractModel> summarisationInfo = (await _summarisationProviderService.GetSummarisedActualsForDataExtractReport(
                new[] { reportServiceContext.CollectionReturnCodeApp, reportServiceContext.CollectionReturnCodeDC, reportServiceContext.CollectionReturnCodeESF },
                cancellationToken)).ToList();
            IEnumerable<string> organisationIds = summarisationInfo?.Select(x => x.OrganisationId).Distinct();
            IEnumerable<DataExtractFcsInfo> fcsInfo = await _fcsProviderService.GetFCSForDataExtractReport(organisationIds, cancellationToken);

            var dataExtractModel = _modelBuilder.BuildModel(summarisationInfo, fcsInfo);
            string csv = await GetCsv(dataExtractModel, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
        }

        private async Task<string> GetCsv(
            IEnumerable<DataExtractModel> dataExtractModel,
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
                        WriteCsvRecords<DataExtractMapper, DataExtractModel>(csvWriter, dataExtractModel);

                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
