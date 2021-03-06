﻿using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model
{
    public class ProviderSubmissionModel
    {
        public long Ukprn { get; set; }

        public int ReturnPeriod { get; set; }

        public string Name { get; set; }

        public bool Expected { get; set; }

        public bool Returned { get; set; }

        public string LatestReturn { get; set; }

        public int? TotalValid { get; set; }

        public int? TotalInvalid { get; set; }

        public int? TotalErrors { get; set; }

        public int? TotalWarnings { get; set; }

        public DateTime? SubmittedDateTime { get; set; }
    }
}
