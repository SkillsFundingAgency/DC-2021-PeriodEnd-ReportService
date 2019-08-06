using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.Fcs
{
    public class FcsContractor
    {
        // Sql Primary Key
        public int Id { get; set; }

        public string OrganisationIdentifier { get; set; }

        public int UkPrn { get; set; }

        public string LegalName { get; set; }

        public IList<FcsContract> FcsContracts { get; set; }
    }
}
