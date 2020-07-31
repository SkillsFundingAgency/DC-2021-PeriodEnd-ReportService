using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.CollectionStats.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.CollectionStats
{
    public class CollectionStatsDataProvider : ICollectionStatsDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private string sql = @"SELECT
                                COALESCE([c].[Name], 'Total') AS CollectionName,
                                COUNT(CASE WHEN [Status] = @completedStatus THEN 1 ELSE NULL END) AS CountOfComplete,
                                COUNT(CASE WHEN [Status] IN @failedStatus THEN 1 ELSE NULL END) AS CountOfFail
                            FROM [Job] j
                            INNER JOIN [Collection] c
                                ON j.CollectionId = c.CollectionId
                            INNER JOIN [CollectionType] ct
                                ON c.CollectionTypeId = ct.CollectionTypeId
                            INNER JOIN FileUploadJobMetaData md
                                ON [j].[JobId] = [md].[JobId]
                            WHERE [CollectionYear] = @collectionYear
                            AND [PeriodNumber] = @periodNumber
                            AND [Status] IN @filterStatus
                            AND [ct].[Type] IN @collectionTypes
                            GROUP BY ROLLUP([c].Name)";

        public CollectionStatsDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<IEnumerable<CollectionStatsModel>> ProvideAsync(int collectionYear, int periodNumber)
        {
            using (var connection = _sqlConnectionFunc())
            {
                return await connection.QueryAsync<CollectionStatsModel>(sql, 
                    new
                    {
                        collectionYear,
                        periodNumber,
                        collectionTypes = new[] { CollectionTypes.ILR, CollectionTypes.EAS, CollectionTypes.ESF },
                        filterStatus = new [] { JobStatus.Completed, JobStatus.Failed, JobStatus.FailedRetry },
                        completedStatus = JobStatus.Completed,
                        failedStatus = new [] { JobStatus.Failed, JobStatus.FailedRetry }
                    });
            }
        }
    }
}
