using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentILRInfo
    {
        public int UkPrn { get; set; }

        public List<AppsMonthlyPaymentLearnerInfo> Learners { get; set; }
    }
}
