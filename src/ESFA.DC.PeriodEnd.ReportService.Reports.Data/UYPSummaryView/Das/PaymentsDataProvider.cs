using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.UYPSummaryView.Das
{
    public class PaymentsDataProvider : IPaymentsDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string paymentsSql = @"SELECT DISTINCT AcademicYear, 
                                                        LearnerReferenceNumber, 
                                                        LearnerUln, 
                                                        LearningAimReference, 
                                                        ReportingAimFundingLineType, 
                                                        CollectionPeriod, 
                                                        Amount, 
                                                        TransactionType, 
                                                        FundingSource, 
                                                        ApprenticeshipId 
                                                    FROM Payments2.Payment 
                                                    WHERE Ukprn = @ukprn and AcademicYear = @academicYear ";

        private readonly string dataLockSql = @"SELECT DISTINCT LearnerReferenceNumber, 
                                                    DataLockFailureId, 
                                                    DeliveryPeriod 
                                                FROM Payments2.DataMatchReport WITH (NOLOCK) 
                                                WHERE Ukprn = @Ukprn";

        private readonly string HBCPInfoSql = @"SELECT DISTINCT LearnerReferenceNumber, 
                                                    DeliveryPeriod, 
                                                    NonPaymentReason 
                                                FROM Payments2.RequiredPaymentEvent 
                                                WHERE Ukprn = @Ukprn";

        private readonly string GetLegalEntityNameSql = @"SELECT DISTINCT Id, 
                                                                LegalEntityName 
                                                            FROM Payments2.Apprenticeship 
                                                            WHERE Ukprn = @Ukprn AND Id IN @pageApprenticeshipIds";

        public PaymentsDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<Payment>> GetPaymentsAsync(int ukprn, int academicYear, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<Payment>(paymentsSql, new { ukprn, academicYear });

                return result.ToList();
            }
        }

        public async Task<ICollection<DataLock>> GetDASDataLockAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<DataLock>(dataLockSql, new { ukprn });

                return result.ToList();
            }
        }

        public async Task<ICollection<HBCPInfo>> GetHBCPInfoAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<HBCPInfo>(HBCPInfoSql, new { ukprn });

                return result.ToList();
            }
        }

        public async Task<IDictionary<long, string>> GetLegalEntityNameAsync(int ukprn, IEnumerable<long> apprenticeshipIds, CancellationToken cancellationToken)
        {
            var uniqueApprenticeshipIds = apprenticeshipIds.Distinct().OrderBy(a => a).ToArray();
            var pageSize = 1000;
            var count = uniqueApprenticeshipIds.Count();

            using (var connection = _sqlConnectionFunc())
            {
                List<ApprenticeshipInfo> result = new List<ApprenticeshipInfo>();

                for (var i = 0; i < count; i += pageSize)
                {
                    IEnumerable<long> pageApprenticeshipIds = uniqueApprenticeshipIds.Skip(i).Take(pageSize).ToArray();
                    result.AddRange(await connection.QueryAsync<ApprenticeshipInfo>(GetLegalEntityNameSql, new { ukprn, pageApprenticeshipIds }));
                }

                return result.ToDictionary(a => a.Id, a => a.LegalEntityName);
            }
        }
    }
}
