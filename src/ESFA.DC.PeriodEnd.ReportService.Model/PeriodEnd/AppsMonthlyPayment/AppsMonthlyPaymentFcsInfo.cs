using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentFcsInfo
    {
        public int UkPrn { get; set; }

        public List<AppsMonthlyPaymentContractInfo> Contracts { get; set; }
    }
}
