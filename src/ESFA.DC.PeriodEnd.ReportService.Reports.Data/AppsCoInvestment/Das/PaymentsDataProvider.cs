using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsCoInvestment.Das
{
    public class PaymentsDataProvider : IPaymentsDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string paymentsSql = @"SELECT 
                                                    FundingSource, 
                                                    TransactionType, 
                                                    AcademicYear, 
                                                    CollectionPeriod, 
                                                    ContractType, 
                                                    DeliveryPeriod, 
                                                    LearnerReferenceNumber, 
                                                    LearnerUln, 
                                                    LearningAimFrameworkCode, 
                                                    LearningAimPathwayCode, 
                                                    LearningAimProgrammeType, 
                                                    LearningAimReference, 
                                                    LearningAimStandardCode, 
                                                    LearningStartDate, 
                                                    Amount, 
                                                    PriceEpisodeIdentifier, 
                                                    SfaContributionPercentage, 
                                                    P.ApprenticeshipId,
                                                    A.LegalEntityName
                                                FROM [Payments2].[Payment] AS P
                                                    LEFT JOIN [Payments2].[Apprenticeship] A ON P.ApprenticeshipId = A.id
                                                WHERE P.Ukprn = @ukprn
                                                AND (FundingSource = 3 or TransactionType IN (1, 2, 3))";

        public PaymentsDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<Payment>> GetPaymentsAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<Payment>(paymentsSql, new { ukprn });

                return result.ToList();
            }
        }
    }
}
