using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary
{
    public abstract class BasePeriodisedValuesProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        protected virtual string Sql { get; }

        protected BasePeriodisedValuesProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public virtual async Task<Dictionary<string, Dictionary<string, decimal?[][]>>> ProvideAsync(long ukprn,
            CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var results = await connection.QueryAsync<PeriodisedValues>(Sql, new { Ukprn = ukprn });

                return BuildDictionary(results);
            }
        }

        private Dictionary<string, Dictionary<string, decimal?[][]>> BuildDictionary(IEnumerable<PeriodisedValues> periodisedValues)
        {
            return periodisedValues
                .GroupBy(pv => pv.FundLine, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key,
                    v => v
                        .GroupBy(ldpv => ldpv.AttributeName, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(k => k.Key, value =>
                                value.Select(pvGroup => pvGroup.Periods).ToArray(),
                            StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}
