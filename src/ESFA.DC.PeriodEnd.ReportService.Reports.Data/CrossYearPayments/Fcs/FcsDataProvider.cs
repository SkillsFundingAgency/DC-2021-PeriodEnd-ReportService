using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.CrossYearPayments.Fcs
{
    public class FcsDataProvider : IFcsDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private const string PaymentsSql = @"SELECT [FspCode]
                                              ,[ContractAllocationNumber]
                                              ,[Period]
                                              ,[Type]
                                              ,[Value]
                                          FROM [dbo].[Payment]
                                          WHERE Ukprn = @ukprn";

        private const string AllocationSql = @"SELECT [ContractNumber]
                                              ,[ContractVersionNumber]
                                              ,[FspCode]
                                              ,[ContractAllocationNumber]
                                              ,[Period]
                                              ,[PlannedValue]
                                              ,[ApprovalTimestamp]
                                          FROM [dbo].[Allocation]
                                          WHERE Ukprn = @ukprn";

        private const string ContractSql = @"SELECT [ContractAllocationNumber]
                                                  ,[FundingStreamPeriodCode]
                                              FROM [dbo].[ContractAllocation]
                                              WHERE DeliveryUKPRN = @ukprn";

        public FcsDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<FcsPayment>> ProvidePaymentsAsync(long ukprn)
        {
            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<FcsPayment>(PaymentsSql, new { ukprn })).ToList();
            }
        }

        public async Task<ICollection<FcsAllocation>> ProvideAllocationsAsync(long ukprn)
        {
            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<FcsAllocation>(AllocationSql, new { ukprn })).ToList();
            }
        }

        public async Task<IDictionary<string, List<string>>> ProviderContractsAsync(long ukprn)
        {
            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<(string ContractAllocationNumber, string FundingStreamPeriodCode)>(ContractSql, new {ukprn}))
                    .GroupBy(x => x.FundingStreamPeriodCode)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.ContractAllocationNumber).ToList(), StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
