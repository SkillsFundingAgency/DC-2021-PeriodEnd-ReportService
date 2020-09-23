using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data
{
    public class FcsAllocation
    {
        public string ContractNumber { get; set; }

        public string FspCode { get; set; }

        public string ContractAllocationNumber { get; set; }

        public int Period { get; set; }

        public decimal PlannedValue { get; set; }

        public DateTime ApprovalTimestamp { get; set; }
    }
}
