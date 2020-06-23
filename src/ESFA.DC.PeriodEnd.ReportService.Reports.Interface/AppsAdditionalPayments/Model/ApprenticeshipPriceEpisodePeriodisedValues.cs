namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model
{
    public class ApprenticeshipPriceEpisodePeriodisedValues
    {
        public int UKPRN { get; set; }

        public string LearnRefNumber { get; set; }

        public int? AimSeqNumber { get; set; }

        public string AttributeName { get; set; }

        public decimal? Period_1 { get; set; }
        public decimal? Period_2 { get; set; }
        public decimal? Period_3 { get; set; }
        public decimal? Period_4 { get; set; }
        public decimal? Period_5 { get; set; }
        public decimal? Period_6 { get; set; }
        public decimal? Period_7 { get; set; }
        public decimal? Period_8 { get; set; }
        public decimal? Period_9 { get; set; }
        public decimal? Period_10{ get; set; }
        public decimal? Period_11{ get; set; }
        public decimal? Period_12 { get; set; }

        /*

        SELECT TOP(1000)
        apepv.*, ape.PriceEpisodeAimSeqNumber
            FROM[Rulebase].[AEC_ApprenticeshipPriceEpisode]
        ape
            left join[Rulebase].[AEC_ApprenticeshipPriceEpisode_PeriodisedValues] apepv on ape.ukprn = apepv.ukprn and ape.LearnRefNumber = apepv.LearnRefNumber and ape.PriceEpisodeIdentifier = apepv.PriceEpisodeIdentifier
            where 
        --ape.ukprn = '10000001' and
            apepv.AttributeName in
        (
            'PriceEpisodeFirstEmp1618Pay',
            'PriceEpisodeSecondEmp1618Pay',
            'PriceEpisodeFirstProv1618Pay',
            'PriceEpisodeSecondProv1618Pay',
            'PriceEpisodeLearnerAdditionalPayment'
            )
            and(
                Period_1 != 0 or
            Period_2 != 0 or
            Period_3 != 0 or
            Period_4 != 0 or
            Period_5 != 0 or
            Period_6 != 0 or
            Period_7 != 0 or
            Period_8 != 0 or
            Period_9 != 0 or
            Period_10 != 0 or
            Period_11 != 0 or
            Period_12 != 0 
        )
        and PriceEpisodeAimSeqNumber is not null
         */
    }
}