using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model
{
    public class HBCPInfo
    {
        public string LearnerReferenceNumber { get; set; }

        public byte CollectionPeriod { get; set; }

        public byte? NonPaymentReason { get; set; }
    }
}
