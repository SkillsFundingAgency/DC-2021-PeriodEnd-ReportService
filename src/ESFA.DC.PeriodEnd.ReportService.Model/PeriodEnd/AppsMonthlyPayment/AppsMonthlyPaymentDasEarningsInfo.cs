using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentDasEarningsInfo
    {
        public long UkPrn { get; set; }

        public List<AppsMonthlyPaymentDasEarningEventModel> Earnings { get; set; }
    }
}
