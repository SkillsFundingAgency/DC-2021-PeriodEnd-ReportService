using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.Fcs
{
    public class FcsContract
    {
        // SQL Primary Key
        public int Id { get; set; }

        // SQL Foreign Key from the Contractor Entity
        public int ContractorId { get; set; }

        public string ContractNumber{ get; set; }

        public string ContractVersionNumber { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

       public IList<FcsContractAllocation> ContractAllocations { get; set; }
    }
}
