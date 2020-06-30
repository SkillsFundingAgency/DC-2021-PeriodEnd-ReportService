using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model
{
    public class AECApprenticeshipPriceEpisodePeriodisedValues
    {
        public string LearnRefNumber { get; set; }

        public int? AimSeqNumber { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public string AttributeName { get; set; }

        public decimal?[] Periods
        {
            get
            {
                return new[]
                {
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
                    Period12,
                };
            }
        }

        public decimal? Period1 { get; set; }

        public decimal? Period2 { get; set; }

        public decimal? Period3 { get; set; }

        public decimal? Period4 { get; set; }

        public decimal? Period5 { get; set; }

        public decimal? Period6 { get; set; }

        public decimal? Period7 { get; set; }

        public decimal? Period8 { get; set; }

        public decimal? Period9 { get; set; }

        public decimal? Period10 { get; set; }

        public decimal? Period11 { get; set; }

        public decimal? Period12 { get; set; }
    }
}
