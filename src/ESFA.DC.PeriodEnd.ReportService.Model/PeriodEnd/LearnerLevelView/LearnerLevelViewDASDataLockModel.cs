using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView
{
    public class LearnerLevelViewDASDataLockModel
    {
        public string CollectionType { get; set; }

        public long UkPrn { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public long LearnerUln { get; set; }

        public byte DataLockFailureId { get; set; }

        public long LearningAimSequenceNumber { get; set; }

        public byte CollectionPeriod { get; set; }

        public byte DataLockSourceId { get; set; }

        public bool IsPayable { get; set; }

        public string LearningAimReference { get; set; }

        public byte DeliveryPeriod { get; set; }

        public DateTime IlrSubmissionDateTime { get; set; }
    }
}
