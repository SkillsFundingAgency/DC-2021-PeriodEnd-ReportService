using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments.Das
{
    public class PaymentsDataProvider : IPaymentsDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string sql = @"
SELECT 
	LearnerReferenceNumber
	,LearnerUln
	,LearningAimReference
	,LearningStartDate
	,LearningAimProgrammeType
	,LearningAimStandardCode
	,LearningAimFrameworkCode
	,LearningAimPathwayCode
	,LearningAimFundingLineType
	,CollectionPeriod 
	,ContractType
	,TransactionType
	,a.LegalEntityName
	,SUM(Amount) as Amount
FROM [Payments2].[Payment] p
LEFT JOIN [Payments2].[Apprenticeship] a ON p.ApprenticeshipId = a.id
WHERE AcademicYear = @academicYear
AND p.Ukprn = @ukprn
AND TransactionType IN (4, 5, 6, 7, 16)
GROUP BY 
	LearnerReferenceNumber
	,learnerUln
	,LearningAimReference
	,LearningStartDate
	,LearningAimProgrammeType
	,LearningAimStandardCode
	,LearningAimFrameworkCode
	,LearningAimPathwayCode
	,LearningAimFundingLineType
	,CollectionPeriod 
	,ContractType
	,TransactionType
	,a.LegalEntityName
ORDER BY LearnerReferenceNumber";

        public PaymentsDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<Payment>> ProvideAsync(int academicYear, long ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var results = await connection.QueryAsync<Payment>(sql, new { ukprn, academicYear });

                return results.ToList();
            }
        }
    }
}