using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Das
{
    public class DasDataProvider : IDasDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string sql = "SELECT ReportingAimFundingLineType AS FundLine, FundingSource, TransactionType, SUM(CASE WHEN DeliveryPeriod = 1 THEN Amount ELSE 0 END) AS Period1, SUM(CASE WHEN DeliveryPeriod = 2 THEN Amount ELSE 0 END) AS Period2, SUM(CASE WHEN DeliveryPeriod = 3 THEN Amount ELSE 0 END) AS Period3, SUM(CASE WHEN DeliveryPeriod = 4 THEN Amount ELSE 0 END) AS Period4, SUM(CASE WHEN DeliveryPeriod = 5 THEN Amount ELSE 0 END) AS Period5, SUM(CASE WHEN DeliveryPeriod = 6 THEN Amount ELSE 0 END) AS Period6, SUM(CASE WHEN DeliveryPeriod = 7 THEN Amount ELSE 0 END) AS Period7, SUM(CASE WHEN DeliveryPeriod = 8 THEN Amount ELSE 0 END) AS Period8, SUM(CASE WHEN DeliveryPeriod = 9 THEN Amount ELSE 0 END) AS Period9, SUM(CASE WHEN DeliveryPeriod = 10 THEN Amount ELSE 0 END) AS Period10, SUM(CASE WHEN DeliveryPeriod = 11 THEN Amount ELSE 0 END) AS Period11, SUM(CASE WHEN DeliveryPeriod = 12 THEN Amount ELSE 0 END) AS Period12 FROM Payments2.Payment WHERE Ukprn = @ukprn AND AcademicYear = 1920 GROUP BY ReportingAimFundingLineType, FundingSource, TransactionType";

        public DasDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<Dictionary<string, Dictionary<int, Dictionary<int, decimal?[][]>>>> Provide(long ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var results = await connection.QueryAsync<DasPeriodisedValues>(sql, new { Ukprn = ukprn });

                return results.ToList()
                    .GroupBy(p => p.FundLine, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(k => k.Key,
                        v => v
                            .GroupBy(fsg => fsg.FundingSource)
                            .ToDictionary(fsgk => fsgk.Key,
                                fs =>
                                    fs
                                        .GroupBy(ttg => ttg.TransactionType)
                                        .ToDictionary(k => k.Key,
                                            tt => tt.Select(pvGroup => pvGroup.Periods).ToArray())),
                        StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
