using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class EasPeriodisedValue : PeriodisedValue
    {
        EasPeriodisedValue
        (
            string fundLine, 
            string attributeName, 
            decimal? Period1,
            decimal? Period2,
            decimal? Period3,
            decimal? Period4,
            decimal? Period5,
            decimal? Period6,
            decimal? Period7,
            decimal? Period8,
            decimal? Period9,
            decimal? Period10,
            decimal? Period11,
            decimal? Period12
        )
        :base
        (
            attributeName,
            Period1,
            Period2,
            Period3,
            Period4,
            Period5,
            Period6,
            Period7,
            Period8,
            Period9,
            Period10,
            Period11,
            Period12
        )
        {
            FundLine = fundLine;
        }

        public string FundLine { get; set; }
    }
}
