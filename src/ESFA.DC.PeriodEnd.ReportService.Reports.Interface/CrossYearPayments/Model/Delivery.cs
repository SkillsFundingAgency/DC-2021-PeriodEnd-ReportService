using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model
{
    public class Delivery
    {
        public string ContractNumber { get; set; }

        public string DeliveryName { get; set; }

        //public ICollection<PeriodDelivery> PeriodDeliveries { get; set; }

        public ICollection<ContractValue> ContractValues { get; set; }

        public ICollection<FSRValue> FSRValues { get; set; }

        public ICollection<FcsPayment> FcsPayments { get; set; }
    }
}
