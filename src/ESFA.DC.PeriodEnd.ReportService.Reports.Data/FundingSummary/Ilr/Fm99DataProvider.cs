﻿using System;
using System.Data.SqlClient;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Ilr
{
    public class Fm99DataProvider : BasePeriodisedValuesProvider, IFm99DataProvider
    {
        protected override string Sql => @"SELECT 
                                            [LD].[FundLine],
                                            [PV].[AttributeName],
                                            SUM([Period_1]) AS Period1,
                                            SUM([Period_2]) AS Period2,
                                            SUM([Period_3]) AS Period3,
                                            SUM([Period_4]) AS Period4,
                                            SUM([Period_5]) AS Period5,
                                            SUM([Period_6]) AS Period6,
                                            SUM([Period_7]) AS Period7,
                                            SUM([Period_8]) AS Period8,
                                            SUM([Period_9]) AS Period9,
                                            SUM([Period_10]) AS Period10,
                                            SUM([Period_11]) AS Period11,
                                            SUM([Period_12]) AS Period12
                                        FROM [Rulebase].[ALB_LearningDelivery_PeriodisedValues] PV
                                        LEFT JOIN [Rulebase].[ALB_LearningDelivery] LD
                                            ON [PV].[UKPRN] = [LD].[UKPRN]
                                            AND [PV].[LearnRefNumber] = [LD].[LearnRefNumber]
                                            AND [PV].AimSeqNumber = [LD].[AimSeqNumber]
                                        WHERE [PV].[UKPRN] = @ukprn
                                        GROUP BY FundLine, AttributeName";

        public Fm99DataProvider(Func<SqlConnection> sqlConnectionFunc) 
            : base(sqlConnectionFunc)
        {
        }
    }
}
