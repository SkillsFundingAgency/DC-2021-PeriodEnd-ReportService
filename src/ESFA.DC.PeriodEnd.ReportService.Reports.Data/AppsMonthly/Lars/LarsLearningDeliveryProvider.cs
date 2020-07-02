using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly.Lars
{
    public class LarsLearningDeliveryProvider : ILarsLearningDeliveryProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;
        private readonly IJsonSerializationService _jsonSerializationService;

        private readonly string sql = @"SELECT  
                                         L.[LearnAimRef]
                                        ,[LearnAimRefTitle]
                                    FROM OPENJSON(@learnAimRefs) 
                                    WITH (LearnAimRef nvarchar(8) '$') J 
                                    INNER JOIN [Core].[LARS_LearningDelivery] L
                                    ON L.[LearnAimRef] = J.[LearnAimRef]";

        public LarsLearningDeliveryProvider(Func<SqlConnection> sqlConnectionFunc, IJsonSerializationService jsonSerializationService)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
            _jsonSerializationService = jsonSerializationService;
        }

        public async Task<ICollection<LarsLearningDelivery>> GetLarsLearningDeliveriesAsync(ICollection<Learner> learners, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var learnAimRefs = _jsonSerializationService.Serialize(learners.SelectMany(l => l.LearningDeliveries.Select(ld => ld.LearnAimRef)).Distinct());

                var commandDefinition = new CommandDefinition(sql, new { learnAimRefs }, cancellationToken: cancellationToken);

                var result = await connection.QueryAsync<LarsLearningDelivery>(commandDefinition);

                return result.ToList();
            }
        }
    }
}