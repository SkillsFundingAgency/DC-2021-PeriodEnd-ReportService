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
    public class AecLearningDeliveryDataProvider : IAecLearningDeliveryDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string sql = @"
SELECT 
       ld.[UKPRN]
      ,ld.[LearnRefNumber]
      ,ld.[LearnAimRef]
      ,ld.[LearnStartDate]
      ,ld.[ProgType]
      ,ld.[StdCode]
      ,ld.[FworkCode]
      ,ld.[PwayCode]
      ,ld.[AimSeqNumber]
      ,AECld.LearnDelEmpIdFirstAdditionalPaymentThreshold
      ,AECld.LearnDelEmpIdSecondAdditionalPaymentThreshold
  FROM [Valid].[LearningDelivery] ld 
  LEFT JOIN [Rulebase].[AEC_LearningDelivery] AECld ON ld.UKPRN = AECld.UKPRN AND ld.LearnRefNumber = AECld.LearnRefNumber AND ld.AimSeqNumber = AECld.AimSeqNumber
  WHERE ld.UKPRN = @ukprn
  ORDER BY ld.LearnRefNumber";

        public AecLearningDeliveryDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<AecLearningDelivery>> ProvideAsync(long ukprn,
            CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var results = await connection.QueryAsync<AecLearningDelivery>(sql, new {ukprn});

                return results.ToList();
            }

        }
    }
}