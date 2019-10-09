using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport.Eas
{
    public class EasSubmissionValue
    {
        public Guid? SubmissionId { get; set; }

        public byte? CollectionPeriod { get; set; }

        public int? PaymentId { get; set; }

        public decimal? PaymentValue { get; set; }

        public int? DevolvedAreaSoF { get; set; }

        //public string PaymentName { get; set; }
        //public string AdjustmentTypeName { get; set; }
        //public EasPaymentValue Period1 { get; set; }
        //public EasPaymentValue Period2 { get; set; }
        //public EasPaymentValue Period3 { get; set; }
        //public EasPaymentValue Period4 { get; set; }
        //public EasPaymentValue Period5 { get; set; }
        //public EasPaymentValue Period6 { get; set; }
        //public EasPaymentValue Period7 { get; set; }
        //public EasPaymentValue Period8 { get; set; }
        //public EasPaymentValue Period9 { get; set; }
        //public EasPaymentValue Period10 { get; set; }
        //public EasPaymentValue Period11 { get; set; }
        //public EasPaymentValue Period12 { get; set; }
    }
}
