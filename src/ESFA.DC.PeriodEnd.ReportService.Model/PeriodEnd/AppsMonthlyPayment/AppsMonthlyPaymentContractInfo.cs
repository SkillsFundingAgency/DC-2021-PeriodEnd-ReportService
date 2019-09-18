using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentContractInfo
    {
        public string ContractNumber { get; set; }

        public string ContractVersionNumber { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public IList<AppsMonthlyPaymentContractAllocationInfo> ContractAllocations { get; set; }

        public AppsMonthlyPaymentContractorInfo Provider { get; set; }
    }
}