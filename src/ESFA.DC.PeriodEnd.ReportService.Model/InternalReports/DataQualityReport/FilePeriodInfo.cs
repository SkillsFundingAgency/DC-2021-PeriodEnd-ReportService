﻿using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport
{
    public sealed class FilePeriodInfo
    {
        public long UKPRN { get; set; }

        public string Filename { get; set; }

        public int PeriodNumber { get; set; }

        public DateTime? SubmittedTime { get; set; }
    }
}
