using ESFA.DC.ILR1920.DataStore.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport
{
    public sealed partial class FileDetailsPerUKPRN
    {
        public int UKPRN { get; set; }
        public string Filename { get; set; }
        public int PeriodNumber { get; set; }
        public DateTime? SubmittedTime { get; set; }
        public bool? Success { get; set; }
    }
}
