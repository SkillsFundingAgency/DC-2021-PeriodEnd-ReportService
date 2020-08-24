using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model
{
    public class OrganisationCollection
    {
        public long Ukprn { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }
    }
}
