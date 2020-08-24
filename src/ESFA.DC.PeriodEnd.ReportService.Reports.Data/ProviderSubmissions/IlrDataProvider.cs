using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.ProviderSubmissions
{
    public class IlrDataProvider : IIlrDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string Sql = @"SELECT
                                         FileName,
                                         Ukprn,
                                         SubmittedTime,
                                         TotalErrorCount,
                                         TotalInvalidLearnersSubmitted,
                                         TotalValidLearnersSubmitted,
                                         TotalWarningCount
                                        FROM FileDetails
                                        WHERE [Filename] IN @fileNames";

        public IlrDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<FileDetails>> ProvideAsync(IEnumerable<ProviderReturnPeriod> providerReturnPeriods)
        {
            var fileNames = providerReturnPeriods.Select(f => f.FileName.Replace(".ZIP", ".XML")).ToList();

            var fileDetails = new List<FileDetails>();

            int count = fileNames.Count;
            int pageSize = 1000;

            using (var connection = _sqlConnectionFunc())
            {
                for (int i = 0; i < count; i += pageSize)
                {
                    var fileNamesInPage = fileNames.Skip(i).Take(pageSize).ToList();

                    var fileDetailsInPage = await connection.QueryAsync<FileDetails>(Sql, new { fileNames = fileNamesInPage });

                    fileDetails.AddRange(fileDetailsInPage);
                }
            }

            return fileDetails;
        }
    }
}
