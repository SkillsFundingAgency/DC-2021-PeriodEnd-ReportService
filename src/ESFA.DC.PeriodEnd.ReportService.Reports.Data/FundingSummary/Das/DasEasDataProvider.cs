using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Das
{
    public class DasEasDataProvider : IDasEasDataProvider
    {
        private readonly Func<SqlConnection> _dasSqlConnection;
        private readonly Func<SqlConnection> _easSqlConnection;
        private readonly int _academicYear = 2021;

        private readonly string _easPaymentsSql = "SELECT Payment_Id AS PaymentId, FL.Name AS FundingLine, AT.Name AS AdjustmentType FROM[dbo].[Payment_Types] PT INNER JOIN FundingLine FL ON PT.FundingLineId = FL.Id INNER JOIN AdjustmentType AT ON PT.AdjustmentTypeId = AT.Id";

        private readonly string _dasSql = "SELECT PaymentType, (CASE WHEN SubmissionCollectionPeriod = 1 THEN Amount ELSE 0 END) AS Period1, (CASE WHEN SubmissionCollectionPeriod = 2 THEN Amount ELSE 0 END) AS Period2, (CASE WHEN SubmissionCollectionPeriod = 3 THEN Amount ELSE 0 END) AS Period3, (CASE WHEN SubmissionCollectionPeriod = 4 THEN Amount ELSE 0 END) AS Period4, (CASE WHEN SubmissionCollectionPeriod = 5 THEN Amount ELSE 0 END) AS Period5, (CASE WHEN SubmissionCollectionPeriod = 6 THEN Amount ELSE 0 END) AS Period6, (CASE WHEN SubmissionCollectionPeriod = 7 THEN Amount ELSE 0 END) AS Period7, (CASE WHEN SubmissionCollectionPeriod = 8 THEN Amount ELSE 0 END) AS Period8, (CASE WHEN SubmissionCollectionPeriod = 9 THEN Amount ELSE 0 END) AS Period9, (CASE WHEN SubmissionCollectionPeriod = 10 THEN Amount ELSE 0 END) AS Period10, (CASE WHEN SubmissionCollectionPeriod = 11 THEN Amount ELSE 0 END) AS Period11, (CASE WHEN SubmissionCollectionPeriod = 12 THEN Amount ELSE 0 END) AS Period12 FROM Payments2.ProviderAdjustmentPayments WHERE Ukprn = @ukprn AND SubmissionAcademicYear = @academicYear";

        public DasEasDataProvider(Func<SqlConnection> dasSqlConnection, Func<SqlConnection> easSqlConnection)
        {
            _dasSqlConnection = dasSqlConnection;
            _easSqlConnection = easSqlConnection;
        }

        public async Task<Dictionary<string, Dictionary<string, decimal?[][]>>> ProvideAsync(long ukprn,
            CancellationToken cancellationToken)
        {
            var paymentTypesDictionary = await ProvideEasData();
            var periodisedValues = await ProvideDasData(ukprn, paymentTypesDictionary);

            return BuildDictionary(periodisedValues);
        }

        private async Task<IEnumerable<PeriodisedValues>> ProvideDasData(long ukprn, Dictionary<int, EasPaymentType> paymentTypeDictionary)
        {
            using (var connection = _dasSqlConnection())
            {
                var results = await connection.QueryAsync<DasEasPeriodisedValues>(_dasSql, new { ukprn, academicYear = _academicYear });

                return results
                    .Where(pv => paymentTypeDictionary.ContainsKey(pv.PaymentType))
                    .Select(pv =>
                    {
                        var paymentType = paymentTypeDictionary[pv.PaymentType];

                        return new PeriodisedValues()
                        {
                            FundLine = paymentType.FundingLine,
                            AttributeName = paymentType.AdjustmentType,
                            Period1 = pv.Period1,
                            Period2 = pv.Period2,
                            Period3 = pv.Period3,
                            Period4 = pv.Period4,
                            Period5 = pv.Period5,
                            Period6 = pv.Period6,
                            Period7 = pv.Period7,
                            Period8 = pv.Period8,
                            Period9 = pv.Period9,
                            Period10 = pv.Period10,
                            Period11 = pv.Period11,
                            Period12 = pv.Period12,
                        };
                    });
            }
        }

        private async Task<Dictionary<int, EasPaymentType>> ProvideEasData()
        {
            using (var connection = _easSqlConnection())
            {
                var results = await connection.QueryAsync<EasPaymentType>(_easPaymentsSql);

                return results?.ToDictionary(x => x.PaymentId, v => v) ?? new Dictionary<int, EasPaymentType>();
            }
        }

        private Dictionary<string, Dictionary<string, decimal?[][]>> BuildDictionary(IEnumerable<PeriodisedValues> periodisedValues)
        {
            return periodisedValues
                .GroupBy(pv => pv.FundLine, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key,
                    v => v
                        .GroupBy(ldpv => ldpv.AttributeName, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(k => k.Key, value =>
                                value.Select(pvGroup => pvGroup.Periods).ToArray(),
                            StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}
