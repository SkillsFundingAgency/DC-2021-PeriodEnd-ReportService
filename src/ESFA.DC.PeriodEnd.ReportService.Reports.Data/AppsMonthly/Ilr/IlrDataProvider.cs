using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly.Ilr
{
    public class IlrDataProvider : IIlrDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string priceEpisodeSql = "SELECT LearnRefNumber, PriceEpisodeIdentifier, PriceEpisodeActualEndDateIncEPA FROM [Rulebase].[AEC_ApprenticeshipPriceEpisode] where Ukprn = @ukprn Order by LearnRefNumber";

        public IlrDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<AecApprenticeshipPriceEpisode>> GetPriceEpisodesAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<AecApprenticeshipPriceEpisode>(priceEpisodeSql, new { ukprn });

                return result.ToList();
            }
        }
    }
}
