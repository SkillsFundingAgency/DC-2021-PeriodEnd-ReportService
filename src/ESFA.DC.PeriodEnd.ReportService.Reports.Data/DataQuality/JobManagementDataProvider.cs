using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.DataQuality
{
    public class JobManagementDataProvider : IJobManagementDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        public JobManagementDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<int> ProvideCollectionIdAsync(string collectionType)
        {
            var sql = "SELECT CollectionId FROM Collection WHERE Name = @collectionType";

            using (var connection = _sqlConnectionFunc())
            {
                return await connection.QueryFirstOrDefaultAsync<int>(sql, new { collectionType });
            }
        }

        public async Task<ICollection<FilePeriodInfo>> ProvideFilePeriodInfoForCollectionAsync(int collectionId)
        {
            var sql = @"SELECT
                            [Ukprn],
                            [FileName],
                            [PeriodNumber],
                            [DateTimeCreatedUtc]
                        FROM [FileUploadJobMetaData] md
                        INNER JOIN [Job] j
                            ON [md].[JobId] = [j].[JobId]
                        WHERE [CollectionId] = @collectionId
                        AND [Ukprn] IS NOT NULL
                        AND [j].[Status] = @completedStatus";

            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<FilePeriodInfo>(sql,
                    new
                    {
                        collectionId,
                        completedStatus = JobStatus.Completed
                    })).ToList();
            }
        }
    }
}
