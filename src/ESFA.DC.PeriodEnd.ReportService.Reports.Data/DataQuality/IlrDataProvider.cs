using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.DataQuality
{
    public class IlrDataProvider : IIlrDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        public IlrDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<RuleStats>> ProvideTop20RuleViolationsAsync()
        {
            var sql = @"SELECT
                        TOP 20
                        [RuleName],
                        COUNT(DISTINCT [UKPRN]) AS ProviderCount,
                        COUNT(DISTINCT [LearnRefNumber]) AS LearnerCount,
                        COUNT(DISTINCT [ErrorMessage]) AS TotalErrorCount
                    FROM [ValidationError]
                    WHERE [Severity] = @errorSeverity
                    GROUP BY [RuleName]
                    ORDER BY COUNT(DISTINCT [ErrorMessage]) DESC, COUNT(DISTINCT [UKPRN]) DESC";

            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<RuleStats>(sql, new { errorSeverity = "E" })).ToList();
            }
        }

        public async Task<ICollection<ProviderSubmission>> ProvideProvidersWithoutValidLearners(CancellationToken cancellationToken)
        {
            var sql = @"SELECT
                            [fd].[Ukprn],
                            MAX(fd.SubmittedTime) AS LatestFileSubmitted
                        FROM FileDetails fd
                        LEFT JOIN [Valid].[Learner] l
                            ON [fd].[UKPRN] = [l].[UKPRN]
                        WHERE [l].[UKPRN] IS NULL
                        GROUP BY [fd].[Ukprn]";

            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<ProviderSubmission>(sql, cancellationToken)).ToList();
            }
        }

        public async Task<ICollection<ProviderCount>> ProvideProvidersWithMostInvalidLearners(CancellationToken cancellationToken)
        {
            var sql = @";WITH TopInvalidProviders_CTE AS
                        (
                            SELECT TOP 10
                                [UKPRN],
                                COUNT([LearnRefNumber]) AS InvalidCount
                            FROM [Invalid].[Learner]
                            GROUP BY [UKPRN]
                            ORDER BY COUNT([LearnRefNumber]) DESC
                        ),
                        LatestSubmission_CTE AS
                        (
                            SELECT
                                UKPRN,
                                MAX(SubmittedTime) AS SubmittedTime
                            FROM FileDetails
                            GROUP BY UKPRN
                        )
                        SELECT
                            [cte].[UKPRN],
                            [InvalidCount],
                            COUNT([LearnRefNumber]) AS ValidCount,
                            [Filename],
                            [fd].[SubmittedTime]
                        FROM TopInvalidProviders_CTE cte
                        LEFT JOIN [Valid].[Learner] l
                            ON [cte].[UKPRN] = [l].[UKPRN]
                        INNER JOIN LatestSubmission_CTE submission_cte
                            ON [cte].[UKPRN] = [submission_cte].[UKPRN]
                        INNER JOIN [FileDetails] fd
                            ON [cte].[UKPRN] = [fd].[UKPRN]
                            AND [submission_cte].[SubmittedTime] = [fd].[SubmittedTime]
                        GROUP BY [cte].[UKPRN], [InvalidCount], [Filename], [fd].[SubmittedTime]
                        ORDER BY [InvalidCount] DESC";

            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<ProviderCount>(sql, cancellationToken)).ToList();
            }
        }
    }
}
