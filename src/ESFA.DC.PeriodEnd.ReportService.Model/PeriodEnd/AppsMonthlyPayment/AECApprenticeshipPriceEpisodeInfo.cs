using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AECApprenticeshipPriceEpisodeInfo
    {
        public string Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public string AimSequenceNumber { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public DateTime? EpisodeStartDate { get; set; }

        public DateTime? PriceEpisodeActualEndDate { get; set; }

        public DateTime? PriceEpisodeActualEndDateIncEPA { get; set; }

        public string PriceEpisodeAgreeId { get; set; }
        
    }
}