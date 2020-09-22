using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data
{
    public class CrossYearDataModel
    {
        public string OrgName { get; set; }

        public ICollection<AppsPayment> Payments { get; set; }

        public ICollection<AppsAdjustmentPayment> AdjustmentPayments { get; set; }

        public ICollection<FcsAllocation> FcsAllocations { get; set; }

        public ICollection<FcsPayment> FcsPayments { get; set; }

        public IDictionary<string, List<string>> FcsContracts { get; set; }
    }
}
