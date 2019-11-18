namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common
{
    public class AECLearningDeliveryPeriodisedValuesInfo
    {
        public int? UKPRN { get; set; }

        public string LearnRefNumber { get; set; }

        public int? AimSeqNumber { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public string AttributeName { get; set; }

        public decimal?[] Periods { get; set; }

        public bool? LearnDelMathEng { get; set; }
    }
}
