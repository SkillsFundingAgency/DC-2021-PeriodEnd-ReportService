using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments.Ilr
{
    public class LearnerDataProvider: ILearnerDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string sql = @"
SELECT 
	l.[UKPRN]
    ,l.[LearnRefNumber]
    ,[ULN]
    ,[FamilyName]
    ,[GivenNames]
    , pslmA.ProvSpecLearnMon AS  ProvSpecLearnMonA
    , pslmB.ProvSpecLearnMon AS ProvSpecLearnMonB 
  FROM [Valid].[Learner] l
  LEFT OUTER JOIN Valid.ProviderSpecLearnerMonitoring AS pslmA 
	ON l.UKPRN = pslmA.UKPRN AND l.LearnRefNumber = pslmA.LearnRefNumber AND pslmA.ProvSpecLearnMonOccur = 'A' AND pslmA.ProvSpecLearnMon IS NOT NULL
  LEFT OUTER JOIN Valid.ProviderSpecLearnerMonitoring AS pslmB 
	ON l.UKPRN = pslmB.UKPRN AND l.LearnRefNumber = pslmB.LearnRefNumber AND pslmB.ProvSpecLearnMonOccur = 'B' AND pslmB.ProvSpecLearnMon IS NOT NULL
  WHERE l.UKPRN = @ukprn
  ORDER BY l.LearnRefNumber";

        public LearnerDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<Learner>> ProvideAsync(long ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var results = await connection.QueryAsync<Learner>(sql, new { ukprn });

                return results.ToList();
            }
        }
    }
}