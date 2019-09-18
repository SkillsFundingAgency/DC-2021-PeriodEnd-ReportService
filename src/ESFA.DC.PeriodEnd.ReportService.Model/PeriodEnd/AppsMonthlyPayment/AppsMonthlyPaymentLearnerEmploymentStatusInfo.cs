
using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentLearnerEmploymentStatusInfo
    {
        public int? Ukprn { get; set; }               // Primary key

        public string LearnRefNumber { get; set; }      // Primary key

        public DateTime? DateEmpStatApp { get; set; }      // Primary key

        public int? EmpStat { get; set; }

        public int? EmpdId { get; set; }

        public string AgreeId { get; set; }
    }
}

