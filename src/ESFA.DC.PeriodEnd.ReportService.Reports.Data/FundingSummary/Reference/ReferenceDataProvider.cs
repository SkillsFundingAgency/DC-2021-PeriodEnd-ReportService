using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Reference
{
    public class ReferenceDataProvider : IReferenceDataProvider
    {
        private readonly string _orgSql = "SELECT Name FROM Org_Details WHERE UKPRN = @ukprn";
        private readonly string _easSql = "SELECT UpdatedOn FROM EAS_Submission WHERE UKPRN = @ukprn";
        private readonly string _ilrSql = "SELECT Filename, SubmittedTime FROM FileDetails WHERE UKPRN = @ukprn ORDER BY SubmittedTime DESC";

        private readonly Func<SqlConnection> _orgSqlFunc;
        private readonly Func<SqlConnection> _easSqlFunc;
        private readonly Func<SqlConnection> _ilrSqlFunc;

        public ReferenceDataProvider(Func<SqlConnection> orgSqlFunc, Func<SqlConnection> easSqlFunc, Func<SqlConnection> ilrSqlFunc)
        {
            _orgSqlFunc = orgSqlFunc;
            _easSqlFunc = easSqlFunc;
            _ilrSqlFunc = ilrSqlFunc;
        }

        public async Task<(string providerName, DateTime? easSubmissionDateTime, string ilrSubmissionFileName, DateTime ilrSubmissionDateTime)> ProvideAsync(long ukprn, CancellationToken cancellationToken)
        {
            var providerNameTask = GetProviderNameAsync(ukprn);
            var easSubmissionDateTimeTask = GetLastestEasSubmissionDateTimeAsync(ukprn);
            var ilrSubmissionFileNameTask = GetLatestIlrSubmissionFileNameAsync(ukprn);

            await Task.WhenAll(providerNameTask, easSubmissionDateTimeTask, ilrSubmissionFileNameTask);

            cancellationToken.ThrowIfCancellationRequested();

            return (providerNameTask.Result, easSubmissionDateTimeTask.Result, ilrSubmissionFileNameTask.Result.Filename, ilrSubmissionFileNameTask.Result.IlrSubmittedDateTime);
        }

        public async Task<string> GetProviderNameAsync(long ukprn)
        {
            using (var connection = _orgSqlFunc())
            {
                return  await connection.QueryFirstOrDefaultAsync<string>(_orgSql, new {ukprn}) ?? string.Empty;
            }
        }

        public async Task<DateTime?> GetLastestEasSubmissionDateTimeAsync(long ukprn)
        {
            using (var connection = _easSqlFunc())
            {
                return await connection.QueryFirstOrDefaultAsync<DateTime?>(_easSql, new { ukprn = ukprn.ToString() });
            }
        }

        public async Task<(string Filename, DateTime IlrSubmittedDateTime)> GetLatestIlrSubmissionFileNameAsync(long ukprn)
        {
            using (var connection = _ilrSqlFunc())
            {
                return await connection.QueryFirstOrDefaultAsync<(string Filename, DateTime SubmittedTime)>(_ilrSql, new { ukprn });
            }
        }
    }
}
