using System;
using System.Data.SqlClient;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Ilr
{
    public class Fm25DataProvider : BasePeriodisedValuesProvider, IFm25DataProvider
    {
        protected override string Sql => "SELECT [L].[FundLine], [PV].[AttributeName], SUM([Period_1]) AS Period1, SUM([Period_2]) AS Period2, SUM([Period_3]) AS Period3, SUM([Period_4]) AS Period4, SUM([Period_5]) AS Period5, SUM([Period_6]) AS Period6, SUM([Period_7]) AS Period7, SUM([Period_8]) AS Period8, SUM([Period_9]) AS Period9, SUM([Period_10]) AS Period10, SUM([Period_11]) AS Period11, SUM([Period_12]) AS Period12 FROM[Rulebase].[FM25_FM35_Learner_PeriodisedValues] PV INNER JOIN[Rulebase].[FM25_Learner] L ON[PV].[UKPRN] = [L].[UKPRN] AND[PV].[LearnRefNumber] = [L].[LearnRefNumber] WHERE[PV].[UKPRN] = @ukprn GROUP BY FundLine, AttributeName";

        public Fm25DataProvider(Func<SqlConnection> sqlConnectionFunc) 
            : base(sqlConnectionFunc)
        {
        }
    }
}
