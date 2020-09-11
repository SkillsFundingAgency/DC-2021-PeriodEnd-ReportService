using System.Collections.Generic;
using System.Linq;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model
{
    public class PeriodDelivery
    {
        public string ReturnPeriod { get; set; }

        public ICollection<ContractValue> ContractValues { get; set; }

        public ICollection<FSRValue> FSRValues { get; set; }

        public decimal FSRReconciliationSubtotal { get; set; }

        public decimal FSRSubtotal => FSRValues.Sum(x => x.Value);
    }
}
