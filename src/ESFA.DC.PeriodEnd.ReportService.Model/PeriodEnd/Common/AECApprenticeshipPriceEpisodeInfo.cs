using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common
{
    public class AECApprenticeshipPriceEpisodeInfo
    {
        public int UkPrn { get; set; }

        public string LearnRefNumber { get; set; }

        public int AimSequenceNumber { get; set; }

        public DateTime? PriceEpisodeActualEndDate { get; set; }

        public string PriceEpisodeAgreeId { get; set; }

        public DateTime? AppAdjLearnStartDate { get; set; }
    }
}