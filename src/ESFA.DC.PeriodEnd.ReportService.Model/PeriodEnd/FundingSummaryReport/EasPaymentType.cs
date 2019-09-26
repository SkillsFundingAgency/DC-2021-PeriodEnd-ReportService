using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class EasPaymentType
    {
        public int? PaymentId { get; set; }

        public string PaymentName { get; set; }

        public bool Fm36 { get; set; }

        public string PaymentTypeDescription { get; set; }

        public int? FundingLineId { get; set; }

        public int? AdjustmentTypeId { get; set; }
    }
}
