using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Mappers;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ActCountReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public sealed class ActCountReport : AbstractInternalReport, IInternalReport
    {
        private readonly IPeriodEndQueryService1920 _ilrPeriodEndService;
        private readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;

        public ActCountReport(
            ILogger logger,
            IDateTimeProvider dateTimeProvider,
            IPeriodEndQueryService1920 ilrPeriodEndService,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IValueProvider valueProvider)
        : base(valueProvider, dateTimeProvider)
        {
            _ilrPeriodEndService = ilrPeriodEndService;
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
        }

        public override string ReportFileName { get; set; } = "ACT Count Report";

        public override string ReportTaskName => ReportTaskNameConstants.InternalReports.ActCountReport;

        public async Task<IEnumerable<ActCountModel>> GenerateModelsAsync(CancellationToken cancellationToken)
        {
            return await _ilrPeriodEndService.GetActCounts();
        }

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            string externalFileName = GetFilename(reportServiceContext);
            IEnumerable<ActCountModel> models = await GenerateModelsAsync(cancellationToken);

            using (MemoryStream ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                {
                    using (var csv = new CsvWriter(writer))
                    {
                        csv.Configuration.RegisterClassMap<ActCountModelMapper>();
                        csv.WriteRecords(models);
                    }
                }

                await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", ms, cancellationToken);
            }
        }
    }
}
