using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.DataQuality
{
    public class OrganisationDataProvider : IOrganisationDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnetionFunc;

        private readonly string Sql = @"SELECT 
                                            [UKPRN],
                                            [Name],
                                            [Status]
                                        FROM Org_Details
                                        WHERE [UKPRN] IN @ukprns";

        public OrganisationDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnetionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<Organisation>> ProvideAsync(ICollection<long> ukprns)
        {
            var organisations = new List<Organisation>();

            if (ukprns == null || !ukprns.Any())
            {
                return organisations;
            }

            int count = ukprns.Count;
            int pageSize = 1000;

            using (var connection = _sqlConnetionFunc())
            {
                for (int i = 0; i < count; i += pageSize)
                {
                    var ukprnsInPage = ukprns.Skip(i).Take(pageSize).ToList();

                    var orgs = await connection.QueryAsync<Organisation>(Sql, new { ukprns = ukprnsInPage });

                    organisations.AddRange(orgs);
                }
            }

            return organisations;
        }
    }
}
