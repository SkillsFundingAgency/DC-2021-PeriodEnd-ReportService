using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model
{
    public class CrossYearPaymentsModel
    {
        public HeaderInfo HeaderInfo { get; set; }

        public ICollection<Delivery> Deliveries { get; set; }

        public FooterInfo FooterInfo { get; set; }
    }
}
