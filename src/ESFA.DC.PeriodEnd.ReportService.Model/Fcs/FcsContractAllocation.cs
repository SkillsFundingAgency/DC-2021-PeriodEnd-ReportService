using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.Fcs
{
    public class FcsContractAllocation
    {
        // SQL Primary Key
        public int Id { get; set; }

        // SQL Foreign Key from the Contract Entity
        public int ContractId { get; set; }

        public string ContractAllocationNumber { get; set; }

        public string FundingStreamCode { get; set; }

        public string Period { get; set; }

        public string PeriodTypeCode { get; set; }

        public string FundingStreamPeriodcode { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
