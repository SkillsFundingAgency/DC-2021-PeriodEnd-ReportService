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

        private readonly string sql = @"SELECT  
                                         L.[LearnAimRef]
                                        ,[LearnAimRefTitle]
                                    FROM OPENJSON(@learnAimRefs) 
                                    WITH (LearnAimRef nvarchar(8) '$') J 
                                    INNER JOIN [Core].[LARS_LearningDelivery] L
                                    ON L.[LearnAimRef] = J.[LearnAimRef]";

        public LarsLearningDeliveryProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<LarsLearningDelivery>> GetLarsLearningDeliveriesAsync(string learnAimRefs, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var commandDefinition = new CommandDefinition(sql, new { learnAimRefs }, cancellationToken: cancellationToken);

                var result = await connection.QueryAsync<LarsLearningDelivery>(commandDefinition);

                return result.ToList();
            }
        }
    }
}