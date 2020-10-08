using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.CrossYearPayments.Apps
{
    public class AppsDataProvider : IAppsDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private const string PaymentsSql = @"SELECT
                                        [AcademicYear],
                                        [CollectionPeriod],
										CASE
											WHEN [AcademicYear] < 1920 THEN [LearningAimFundingLineType]
											ELSE [ReportingAimFundingLineType]
										END AS FundingLineType,
                                        [DeliveryPeriod],
                                        [Amount]
                                    FROM [Payments2].[Payment]
                                    WHERE Ukprn = @ukprn";

        private const string AdjustmentPaymentsSql = @"SELECT
                                                            [CollectionPeriodName],
                                                            [SubmissionAcademicYear] AS AcademicYear,
                                                            [SubmissionCollectionPeriod] AS CollectionPeriod,
                                                            [PaymentType],
                                                            [Amount]
                                                        FROM [Payments2].[ProviderAdjustmentPayments]
                                                        WHERE Ukprn = @ukprn"; 

        public AppsDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<AppsPayment>> ProvidePaymentsAsync(long ukprn)
        {
            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<AppsPayment>(PaymentsSql, new { ukprn })).ToList();
            }
        }

        public async Task<ICollection<AppsAdjustmentPayment>> ProvideAdjustmentPaymentsAsync(long ukprn)
        {
            using (var connection = _sqlConnectionFunc())
            {
                return (await connection.QueryAsync<AppsAdjustmentPayment>(AdjustmentPaymentsSql, new { ukprn })).ToList();
            }
        }
    }
}
