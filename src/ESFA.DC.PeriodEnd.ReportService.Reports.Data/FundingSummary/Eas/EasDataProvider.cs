using System;
using System.Data.SqlClient;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Eas
{
    public class EasPeriodisedValuesProvider : BasePeriodisedValuesProvider, IEasDataProvider
    {
        protected override string Sql => "  SELECT [FL].Name AS FundLine, [AT].Name AS AttributeName, SUM(CASE WHEN [SV].CollectionPeriod = 1 THEN[SV].PaymentValue ELSE 0 END) AS Period1, SUM(CASE WHEN[SV].CollectionPeriod = 2 THEN[SV].PaymentValue ELSE 0 END) AS Period2, SUM(CASE WHEN[SV].CollectionPeriod = 3 THEN[SV].PaymentValue ELSE 0 END) AS Period3, SUM(CASE WHEN[SV].CollectionPeriod = 4 THEN[SV].PaymentValue ELSE 0 END) AS Period4, SUM(CASE WHEN[SV].CollectionPeriod = 5 THEN[SV].PaymentValue ELSE 0 END) AS Period5, SUM(CASE WHEN[SV].CollectionPeriod = 6 THEN[SV].PaymentValue ELSE 0 END) AS Period6, SUM(CASE WHEN[SV].CollectionPeriod = 7 THEN[SV].PaymentValue ELSE 0 END) AS Period7, SUM(CASE WHEN[SV].CollectionPeriod = 8 THEN[SV].PaymentValue ELSE 0 END) AS Period8, SUM(CASE WHEN[SV].CollectionPeriod = 9 THEN[SV].PaymentValue ELSE 0 END) AS Period9, SUM(CASE WHEN[SV].CollectionPeriod = 10 THEN[SV].PaymentValue ELSE 0 END) AS Period10, SUM(CASE WHEN[SV].CollectionPeriod = 11 THEN[SV].PaymentValue ELSE 0 END) AS Period11, SUM(CASE WHEN[SV].CollectionPeriod = 12 THEN[SV].PaymentValue ELSE 0 END) AS Period12 FROM EAS_Submission_Values[SV] INNER JOIN EAS_Submission [S] ON[SV].[Submission_Id] = [S].[Submission_Id] AND[SV].[CollectionPeriod] = [S].[CollectionPeriod] INNER JOIN Payment_Types[PT] ON[SV].[Payment_Id] = [PT].[Payment_Id] INNER JOIN AdjustmentType[AT] ON [PT].[AdjustmentTypeId] = [AT].[Id] INNER JOIN FundingLine [FL] ON [PT].[FundingLineId] = [FL].[Id] WHERE[S].[UKPRN] = @ukprn GROUP BY[FL].[Name], [AT].[Name]";

        public EasPeriodisedValuesProvider(Func<SqlConnection> sqlConnectionFunc) 
            : base(sqlConnectionFunc)
        {
        }
    }
}
