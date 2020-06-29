using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly.Fcs
{
    public class FcsDataProvider : IFcsDataProvider
    {
        private readonly Func<SqlConnection> _funcSqlConnection;

        private readonly DateTime _academicYearStartDate = new DateTime(2020, 08, 01);
        private readonly DateTime _academicYearEndDate = new DateTime(2021, 07, 31);

        private readonly string _sql = "SELECT ContractAllocationNumber, FundingStreamPeriodCode AS FundingStreamPeriod  FROM ContractAllocation WHERE DeliveryUkprn = @ukprn AND StartDate <= @academicYearEndDate AND(EndDate IS NULL OR EndDate >= @academicYearStartDate)";

        public FcsDataProvider(Func<SqlConnection> funcSqlConnection)
        {
            _funcSqlConnection = funcSqlConnection;
        }

        public async Task<ICollection<ContractAllocation>> GetContractAllocationsAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _funcSqlConnection())
            {
                var result = await connection.QueryAsync<ContractAllocation>(_sql, new { ukprn, academicYearStartDate = _academicYearStartDate, academicYearEndDate = _academicYearEndDate });

                return result.ToList();
            }
        }
    }
}
