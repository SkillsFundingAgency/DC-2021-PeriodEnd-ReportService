using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentDASInfo
    {
        public long UkPrn { get; set; }

        public List<AppsMonthlyPaymentDasPaymentModel> Payments { get; set; }
    }
}
