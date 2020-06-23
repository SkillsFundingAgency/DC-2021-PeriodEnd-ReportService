using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly.Lars
{
    public class LarsLearningDeliveryProvider : ILarsLearningDeliveryProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string sql = "SELECT LearnAimRef, LearnAimRefTitle FROM Core.LARS_LearningDelivery where LearnAimRef IN @learnAimRefs";

        public LarsLearningDeliveryProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<LarsLearningDelivery>> GetLarsLearningDeliveriesAsync(ICollection<Learner> learners, CancellationToken cancellationToken)
        {
            var learnaimRefs = learners.SelectMany(l => l.LearningDeliveries.Select(ld => ld.LearnAimRef)).Distinct();

            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<LarsLearningDelivery>(sql, new { learnaimRefs });

                return result.ToList();
            }
        }
    }
}