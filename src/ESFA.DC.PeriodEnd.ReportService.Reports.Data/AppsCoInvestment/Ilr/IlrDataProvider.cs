using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsCoInvestment.Ilr
{
    public class IlrDataProvider : IIlrDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string priceEpisodeSql = $@"SELECT 
	                                                    PeriodisedValues.LearnRefNumber,
	                                                    COALESCE(PriceEpisode.PriceEpisodeAimSeqNumber, 0) AS [AimSeqNumber],
	                                                     PeriodisedValues.[AttributeName],
	                                                     PeriodisedValues.[Period_1] AS Period1,
	                                                     PeriodisedValues.[Period_2] AS Period2,
	                                                     PeriodisedValues.[Period_3] AS Period3,
	                                                     PeriodisedValues.[Period_4] AS Period4,
	                                                     PeriodisedValues.[Period_5] AS Period5,
	                                                     PeriodisedValues.[Period_6] AS Period6,
	                                                     PeriodisedValues.[Period_7] AS Period7,
	                                                     PeriodisedValues.[Period_8] AS Period8,
	                                                     PeriodisedValues.[Period_9] AS Period9,
	                                                     PeriodisedValues.[Period_10] AS Period10,
	                                                     PeriodisedValues.[Period_11] AS Period11,
	                                                     PeriodisedValues.[Period_12] AS Period12
                                                    FROM Rulebase.AEC_ApprenticeshipPriceEpisode_PeriodisedValues AS PeriodisedValues
                                                        INNER JOIN Rulebase.AEC_ApprenticeshipPriceEpisode AS PriceEpisode 
                                                            ON ((PeriodisedValues.UKPRN = PriceEpisode.UKPRN) 
	                                                            AND (PeriodisedValues.LearnRefNumber = PriceEpisode.LearnRefNumber))
                                                                AND (PeriodisedValues.PriceEpisodeIdentifier = PriceEpisode.PriceEpisodeIdentifier)
                                                    WHERE 
                                                        (PeriodisedValues.UKPRN = @ukprn) AND
                                                        (PeriodisedValues.AttributeName = '{AttributeConstants.Fm36PriceEpisodeCompletionPaymentAttributeName}')";

        public IlrDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<AECApprenticeshipPriceEpisodePeriodisedValues>> GetAecPriceEpisodePeriodisedValuesAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<AECApprenticeshipPriceEpisodePeriodisedValues>(priceEpisodeSql, new { ukprn });

                return result.ToList();
            }
        }
    }
}
