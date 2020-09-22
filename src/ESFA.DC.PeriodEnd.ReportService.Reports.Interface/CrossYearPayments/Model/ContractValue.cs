using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model
{
    public class ContractValue : IValue
    {
        public int DeliveryPeriod { get; set; }

        public decimal Value { get; set; }

        public DateTime ApprovalTimestamp { get; set; }
    }
}
