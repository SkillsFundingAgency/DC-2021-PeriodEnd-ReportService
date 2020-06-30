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
    public class AppsPriceEpisodePeriodisedValuesDataProvider : IAppsPriceEpisodePeriodisedValuesDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string sql = @"
SELECT
	apepv.LearnRefNumber
	,ape.PriceEpisodeAimSeqNumber as AimSeqNumber
	,apepv.AttributeName
	,apepv.Period_1
	,apepv.Period_2
	,apepv.Period_3
	,apepv.Period_4
	,apepv.Period_5
	,apepv.Period_6
	,apepv.Period_7
	,apepv.Period_8
	,apepv.Period_9
	,apepv.Period_10
	,apepv.Period_11
	,apepv.Period_12
FROM [Rulebase].[AEC_ApprenticeshipPriceEpisode] ape
LEFT JOIN [Rulebase].[AEC_ApprenticeshipPriceEpisode_PeriodisedValues] apepv 
	ON ape.ukprn = apepv.ukprn AND ape.LearnRefNumber = apepv.LearnRefNumber AND ape.PriceEpisodeIdentifier = apepv.PriceEpisodeIdentifier
WHERE ape.ukprn = @ukprn
	AND apepv.AttributeName IN
		(
		'PriceEpisodeFirstEmp1618Pay',
		'PriceEpisodeSecondEmp1618Pay',
		'PriceEpisodeFirstProv1618Pay',
		'PriceEpisodeSecondProv1618Pay',
		'PriceEpisodeLearnerAdditionalPayment')
	AND (
		Period_1 != 0 OR
		Period_2 != 0 OR
		Period_3 != 0 OR
		Period_4 != 0 OR
		Period_5 != 0 OR
		Period_6 != 0 OR
		Period_7 != 0 OR
		Period_8 != 0 OR
		Period_9 != 0 OR
		Period_10 != 0 OR
		Period_11 != 0 OR
		Period_12 != 0)
	AND PriceEpisodeAimSeqNumber IS NOT NULL";

        public AppsPriceEpisodePeriodisedValuesDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<ApprenticeshipPriceEpisodePeriodisedValues>> ProvideAsync(long ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var results = await connection.QueryAsync<ApprenticeshipPriceEpisodePeriodisedValues>(sql, new { ukprn });

                return results.ToList();
            }

        }
    }
}