using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView
{
    public class LearnerLevelViewHBCPModel
    {
        public long UkPrn { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public long LearnerUln { get; set; }

        public byte CollectionPeriod { get; set; }

        public string LearningAimReference { get; set; }

        public byte DeliveryPeriod { get; set; }

        public int? NonPaymentReason { get; set; }
    }
}
