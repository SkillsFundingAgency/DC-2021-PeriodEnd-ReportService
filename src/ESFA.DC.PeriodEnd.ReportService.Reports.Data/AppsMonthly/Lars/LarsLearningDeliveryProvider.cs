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

        private readonly string sql = "SELECT LearnAimRef, LearnAimRefTitle FROM Core.LARS_LearningDelivery where LearnAimRef IN @learnaimRefs";

        public LarsLearningDeliveryProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<LarsLearningDelivery>> GetLarsLearningDeliveriesAsync(ICollection<Learner> learners, CancellationToken cancellationToken)
        {
            var learnaimRefsAll = learners.SelectMany(l => l.LearningDeliveries.Select(ld => ld.LearnAimRef)).Distinct();

            int learnaimRefCount = learnaimRefsAll.Count();

            int restrictedSize = 2000;

            int NumberOfTimesTobeExecuted = (int)Math.Ceiling((double)learnaimRefCount / restrictedSize);

            var larsLearningDeliveries = new List<LarsLearningDelivery>();

            using (var connection = _sqlConnectionFunc())
            {
                for (int i = 1; i <= NumberOfTimesTobeExecuted; i++)
                {
                    var learnaimRefs = learnaimRefsAll.Skip((i - 1) * restrictedSize).Take(restrictedSize);

                    var result = await connection.QueryAsync<LarsLearningDelivery>(sql, new { learnaimRefs });

                    larsLearningDeliveries.AddRange(result);

                }

                return larsLearningDeliveries;
            }
        }
    }
}