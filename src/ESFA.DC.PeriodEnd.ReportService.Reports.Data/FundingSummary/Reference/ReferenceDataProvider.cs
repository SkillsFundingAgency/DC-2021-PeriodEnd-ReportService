using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Reference
{
    public class ReferenceDataProvider
    {
        private readonly string _orgSql = "SELECT Name FROM Org_Details WHERE UKPRN = @ukprn";
        private readonly string _easSql = "SELECT UpdatedOn FROM EAS_Submission WHERE UKPRN = @ukprn";
        private readonly string _ilrSql = "SELECT Filename FROM FileDetails WHERE UKPRN = @ukprn ORDER BY SubmittedTime DESC";

        private readonly Func<SqlConnection> _organisationSqlFunc;
        private readonly Func<SqlConnection> _easSqlFunc;
        private readonly Func<SqlConnection> _ilrSqlFunc;

        public ReferenceDataProvider(Func<SqlConnection> organisationSqlFunc, Func<SqlConnection> easSqlFunc, Func<SqlConnection> ilrSqlFunc)
        {
            _organisationSqlFunc = organisationSqlFunc;
            _easSqlFunc = easSqlFunc;
            _ilrSqlFunc = ilrSqlFunc;
        }

        public async Task<(string, DateTime?, string)> Provide(long ukprn, CancellationToken cancellationToken)
        {
            var providerName = await GetProviderNameAsync(ukprn);
            var easSubmissionDateTime = await GetLastestEasSubmissionDateTimeAsync(ukprn);
            var ilrSubmissionFileName = await GetLatestIlrSubmissionFileNameAsync(ukprn);

            cancellationToken.ThrowIfCancellationRequested();

            return (providerName, easSubmissionDateTime, ilrSubmissionFileName);
        }

        public async Task<string> GetProviderNameAsync(long ukprn)
        {
            using (var connection = _organisationSqlFunc())
            {
                return  await connection.QueryFirstOrDefaultAsync<string>(_orgSql, new {ukprn}) ?? string.Empty;
            }
        }

        public async Task<DateTime?> GetLastestEasSubmissionDateTimeAsync(long ukprn)
        {
            using (var connection = _easSqlFunc())
            {
                return await connection.QueryFirstOrDefaultAsync<DateTime>(_easSql, new { ukprn = ukprn.ToString() });
            }
        }

        public async Task<string> GetLatestIlrSubmissionFileNameAsync(long ukprn)
        {
            using (var connection = _ilrSqlFunc())
            {
                return await connection.QueryFirstOrDefaultAsync<string>(_ilrSql, new { ukprn }) ?? string.Empty;
            }
        }
    }
}
