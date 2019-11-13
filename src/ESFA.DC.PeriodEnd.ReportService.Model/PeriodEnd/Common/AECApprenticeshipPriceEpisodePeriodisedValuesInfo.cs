namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common
{
    public class AECApprenticeshipPriceEpisodePeriodisedValuesInfo
    {
        public int? UKPRN { get; set; }

        public string LearnRefNumber { get; set; }

        public int? AimSeqNumber { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public string AttributeName { get; set; }

        public decimal?[] Periods { get; set; }

        public decimal? Period1 => Periods[0];

        public decimal? Period2 => Periods[1];

        public decimal? Period3 => Periods[2];

        public decimal? Period4 => Periods[3];

        public decimal? Period5 => Periods[4];

        public decimal? Period6 => Periods[5];

        public decimal? Period7 => Periods[6];

        public decimal? Period8 => Periods[7];

        public decimal? Period9 => Periods[8];

        public decimal? Period10 => Periods[9];

        public decimal? Period11 => Periods[10];

        public decimal? Period12 => Periods[11];
    }
}
