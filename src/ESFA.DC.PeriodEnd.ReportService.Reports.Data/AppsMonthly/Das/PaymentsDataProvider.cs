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

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly.Das
{
    public class PaymentsDataProvider : IPaymentsDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string paymentsSql = "SELECT LearnerReferenceNumber, LearnerUln, LearningAimReference, LearningStartDate, LearningAimProgrammeType ,LearningAimStandardCode ,LearningAimFrameworkCode ,LearningAimPathwayCode ,ReportingAimFundingLineType ,PriceEpisodeIdentifier ,ContractType ,EarningEventId ,CollectionPeriod,DeliveryPeriod ,Amount ,TransactionType,FundingSource from Payments2.Payment WHERE Ukprn = @ukprn and AcademicYear = @academicYear ";

        private readonly string earningsSql = "SELECT EventId, LearningAimSequenceNumber AS AimSequenceNumber FROM Payments2.EarningEvent where Ukprn = @ukprn";

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

        public async Task<ICollection<Earning>> GetEarningsAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<Earning>(earningsSql, new { ukprn });

                return result.ToList();
            }
        }
    }
}
