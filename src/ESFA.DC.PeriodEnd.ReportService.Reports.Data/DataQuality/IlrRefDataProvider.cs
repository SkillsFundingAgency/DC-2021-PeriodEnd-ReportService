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
    public class IlrRefDataProvider : IIlrRefDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private string Sql = @"SELECT
                                [Rulename],
                                [Message]
                              FROM [dbo].[Rules]";

        public IlrRefDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<ValidationRule>> ProvideAsync(CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<ValidationRule>(Sql, cancellationToken)).ToList();
            }
        }
    }
}
