using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.CrossYearPayments.Org
{
    public class OrgDataProvider : IOrgDataProvider
    {
        private readonly string _orgSql = "SELECT Name FROM Org_Details WHERE UKPRN = @ukprn";

        private readonly Func<SqlConnection> _sqlConnectionFunc;

        public OrgDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<string> ProvideAsync(long ukprn)
        {
            using (var connection = _sqlConnectionFunc())
            {
                return await connection.QueryFirstOrDefaultAsync<string>(_orgSql, new { ukprn }) ?? string.Empty;
            }
        }
    }
}