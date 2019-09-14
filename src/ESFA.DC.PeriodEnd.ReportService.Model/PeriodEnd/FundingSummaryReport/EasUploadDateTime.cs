using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public struct EasUploadDateTime
    {
        public EasUploadDateTime(DateTime? uploadDateTime)
        {
            UploadDateTime = null;
        }

        public DateTime? UploadDateTime { get; set; }
    }
}