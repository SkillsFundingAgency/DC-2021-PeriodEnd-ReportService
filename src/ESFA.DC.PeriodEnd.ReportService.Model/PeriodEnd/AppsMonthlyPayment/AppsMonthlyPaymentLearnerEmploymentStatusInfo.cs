
using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentLearnerEmploymentStatusInfo
    {
        public string Ukprn { get; set; }               // Primary key

        public string LearnRefNumber { get; set; }      // Primary key

        public DateTime DateEmpStatApp { get; set; }      // Primary key

        public string EmpStat { get; set; }

        public string EmpdId { get; set; }

        public string AgreeId { get; set; }
    }
}

