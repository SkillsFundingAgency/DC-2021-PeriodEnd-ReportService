using System;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders
{
    public class AecApprenticeshipPriceEpisodeBuilder : AbstractBuilder<AecApprenticeshipPriceEpisode>
    {
        public const string PriceEpisodeIdentifier = "PriceEpisodeIdentifier-01/08/2020";

        public static DateTime PriceEpisodeActualEndDate { get;  } = new DateTime(31, 8, 2020);

        public AecApprenticeshipPriceEpisodeBuilder()
        {
            modelObject = new AecApprenticeshipPriceEpisode()
            {
                PriceEpisodeIdentifier = PriceEpisodeIdentifier,
                PriceEpisodeActualEndDateIncEPA = PriceEpisodeActualEndDate,
            };
        }
    }
}
