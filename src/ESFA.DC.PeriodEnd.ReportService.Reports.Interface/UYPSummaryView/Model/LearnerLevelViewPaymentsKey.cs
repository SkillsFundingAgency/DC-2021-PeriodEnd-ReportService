using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model
{
    public struct LearnerLevelViewPaymentsKey
    {
        public LearnerLevelViewPaymentsKey(string learnerRefNum, string paymentFundingLineType)
        {
            LearnerReferenceNumber = learnerRefNum;
            PaymentFundingLineType = paymentFundingLineType;
        }

        public string LearnerReferenceNumber { get; set; }

        public string PaymentFundingLineType { get; set; }
    }
}
