using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Fcs
{
    public class FcsDataProvider : IFcsDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly DateTime _academicYearStartDate = new DateTime(2020, 08, 01);
        private readonly DateTime _academicYearEndDate = new DateTime(2021, 07, 31);

        private readonly string _sql =
            "SELECT FundingStreamPeriodCode, STRING_AGG(ContractAllocationNumber, ';') WITHIN GROUP (ORDER BY ContractAllocationNumber DESC) AS ContractAllocationNumbers FROM ContractAllocation WHERE DeliveryUkprn = @ukprn AND (EndDate IS NULL OR EndDate >= @academicYearStartDate) GROUP BY FundingStreamPeriodCode";

        public FcsDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<IDictionary<string, string>> Provide(long ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var results = await connection.QueryAsync<(string FundingStreamPeriodCode, string ContractAllocationNumbers)>(_sql, new { ukprn, academicYearStartDate = _academicYearStartDate, academicYearEndDate = _academicYearEndDate });

                return results.ToDictionary(k => k.FundingStreamPeriodCode, v => v.ContractAllocationNumbers,
                    StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
