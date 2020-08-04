using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.ProviderSubmissions
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

        public async Task<ICollection<ProviderReturnPeriod>> ProvideReturnersAndPeriodsAsync(int collectionId, int returnPeriod)
        {
            var sql = @";WITH MAXID_CTE AS
                        (
                            SELECT
                                MAX([j].[JobId]) AS JobId,
                                [j].[Ukprn]
                            FROM FileUploadJobMetaData fumd
                            INNER JOIN Job j
                                ON fumd.JobId = j.JobId
                            WHERE j.Ukprn IS NOT NULL
                            AND j.[Status] = @completedStatus
                            AND PeriodNumber <= @returnPeriod
                            AND CollectionId = @collectionId
                            GROUP BY Ukprn
                        )
                        SELECT
                            [j].[Ukprn],
                            [PeriodNumber] AS ReturnPeriod,
                            [FileName]
                        FROM FileUploadJobMetaData fumd
                        INNER JOIN Job j
                            ON fumd.JobId = j.JobId
                        INNER JOIN MAXID_CTE cte
                            ON [cte].[Ukprn] = [j].[Ukprn]
                            AND [cte].[JobId] = [j].[JobId]
                        ORDER BY Ukprn ASC";

            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<ProviderReturnPeriod>(sql,
                    new
                    {
                        collectionId,
                        returnPeriod,
                        completedStatus = JobStatus.Completed
                    })).ToList();
            }
        }

        public async Task<ICollection<OrganisationCollection>> ProvideExpectedReturnersUKPRNsAsync(int collectionId)
        {
            var sql = @"SELECT
                            Ukprn,
                            StartDateTimeUtc AS [Start],
                            EndDateTimeUtc AS [End]
                        FROM OrganisationCollection
                        WHERE CollectionId = @collectionId";

            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<OrganisationCollection>(sql, new {collectionId})).ToList();
            }
        }

        public async Task<ICollection<long>> ProvideActualReturnersUKPRNsAsync(int collectionId, int returnPeriod)
        {
            var sql = @"SELECT
                        Ukprn
                    FROM FileUploadJobMetaData fumd
                    INNER JOIN Job j
                        ON fumd.JobId = j.JobId
                    WHERE j.Ukprn IS NOT NULL
                        AND CollectionId = @collectionId
                        AND PeriodNumber = @returnPeriod
                        AND j.[Status] = @completedStatus
                    GROUP BY UKPRN";

            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<long>(sql, 
                    new
                {
                    collectionId,
                    returnPeriod,
                    completedStatus = JobStatus.Completed
                })).ToList();
            }
        }
    }
}
