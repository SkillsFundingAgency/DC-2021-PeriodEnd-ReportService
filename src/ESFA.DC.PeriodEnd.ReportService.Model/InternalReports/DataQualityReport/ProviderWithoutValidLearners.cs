﻿using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport
{
    public sealed class ProviderWithoutValidLearners
    {
        public long Ukprn { get; set; }

        public DateTime? LatestFileSubmitted { get; set; }

        public string Name { get; set; }
    }
}
