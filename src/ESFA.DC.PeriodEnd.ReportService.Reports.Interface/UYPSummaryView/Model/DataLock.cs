using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model
{
    public class DataLock
    {
        public string LearnerReferenceNumber { get; set; }

        public byte CollectionPeriod { get; set; }

        public byte DataLockFailureId { get; set; }
    }
}
