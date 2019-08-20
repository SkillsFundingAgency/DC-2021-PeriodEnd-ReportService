using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentContractInfo
    {
        public string ContractNumber { get; set; }

        public string ContractVersionNumber { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public IList<AppsMonthlyPaymentContractAllocation> ContractAllocations { get; set; }

        public AppsMonthlyPaymentContractorInfo Provider { get; set; }
    }

    public class AppsMonthlyPaymentContractorInfo
    {
        public string UkPrn { get; set; }

        public string OrganisationIdentifier { get; set; }
   
        public string LegalName { get; set; }
    }

    public class AppsMonthlyPaymentContractAllocation
    {
        public string ContractAllocationNumber { get; set; }

        public string FundingStreamCode { get; set; }

        public string Period { get; set; }

        public string PeriodTypeCode { get; set; }

        public string FundingStreamPeriodCode { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }
    }
}