using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model
{
    public class Delivery
    {
        public string ContractNumber { get; set; }

        public string DeliveryName { get; set; }

        public ICollection<PeriodDelivery> PeriodDeliveries { get; set; }
    }
}
