namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport.PeriodisedValues
{
    public class PeriodisedValue
    {
        public PeriodisedValue()
        {
        }

        public PeriodisedValue(
            string attributeName,
            decimal? period1,
            decimal? period2,
            decimal? period3,
            decimal? period4,
            decimal? period5,
            decimal? period6,
            decimal? period7,
            decimal? period8,
            decimal? period9,
            decimal? period10,
            decimal? period11,
            decimal? period12)
        {
            AttributeName = attributeName;
            Period1 = period1;
            Period2 = period2;
            Period3 = period3;
            Period4 = period4;
            Period5 = period5;
            Period6 = period6;
            Period7 = period7;
            Period8 = period8;
            Period9 = period9;
            Period10 = period10;
            Period11 = period11;
            Period12 = period12;
        }

        public string AttributeName { get; set; }

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