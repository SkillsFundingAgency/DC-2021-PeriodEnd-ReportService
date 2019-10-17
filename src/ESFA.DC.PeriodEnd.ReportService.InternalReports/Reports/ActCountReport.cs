using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Dapper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR1920.DataStore.EF.Valid;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Mappers;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ActCountReport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public sealed class ActCountReport : AbstractInternalReport, IInternalReport
    {
        private const string QUERY =
            "SELECT ld.UKPRN Ukprn, Count(case when ldf.LearnDelFAMCode = '1' then 1 else NULL end) ActCountOne, Count(case when ldf.LearnDelFAMCode = '2' then 1 else NULL end) ActCountTwo FROM [Valid].[LearningDelivery] ld INNER JOIN [Valid].[LearningDeliveryFAM] ldf on ldf.UKPRN = ld.UKPRN and ldf.LearnRefNumber = ld.LearnRefNumber AND ldf.AimSeqNumber = ld.AimSeqNumber WHERE LearnAimRef = 'ZPROG001' and ldf.LearnDelFAMType = 'ACT' and ld.FundModel = 36 GROUP BY ld.UKPRN";

        private readonly DbContextOptions<ILR1920_DataStoreEntitiesValid> _dataStoreOptions;
        private readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;

        public ActCountReport(
            ILogger logger,
            IDateTimeProvider dateTimeProvider,
            DbContextOptions<ILR1920_DataStoreEntitiesValid> dataStoreOptions,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IValueProvider valueProvider)
        : base(valueProvider, dateTimeProvider)
        {
            _dataStoreOptions = dataStoreOptions;
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
        }

        public override string ReportFileName { get; set; } = "ACT Count Report";

        public override string ReportTaskName => ReportTaskNameConstants.InternalReports.ActCountReport;

        public async Task<IEnumerable<ActCountModel>> GenerateModelsAsync(CancellationToken cancellationToken)
        {
            using (SqlConnection sqlConnection = new SqlConnection(_dataStoreOptions.FindExtension<SqlServerOptionsExtension>().ConnectionString))
            {
                return (await sqlConnection.QueryAsync<ActCountModel>(QUERY, cancellationToken)).OrderBy(x => x.Ukprn).ToList();
            }
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
