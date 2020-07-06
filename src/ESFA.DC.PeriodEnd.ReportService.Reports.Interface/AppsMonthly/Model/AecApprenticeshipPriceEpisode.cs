using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model
{
    public class AecApprenticeshipPriceEpisode
    {
        public string LearnRefNumber { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public DateTime? PriceEpisodeActualEndDateIncEPA { get; set; }
    }
}
