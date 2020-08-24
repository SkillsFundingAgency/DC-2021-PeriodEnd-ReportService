using System.Collections.Generic;
using ESFA.DC.CollectionsManagement.Models;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model
{
    public class ProviderSubmissionsReferenceData
    {
        public ICollection<ProviderReturnPeriod> ProviderReturns { get; set; }

        public ICollection<FileDetails> FileDetails { get; set; }

        public ICollection<Organisation> OrgDetails { get; set; }

        public ICollection<OrganisationCollection> ExpectedReturns { get; set; }

        public ICollection<long> ActualReturners { get; set; }

        public List<ReturnPeriod> ILRPeriodsAdjustedTimes { get; set; }

        public int ReturnPeriod { get; set; }
    }
}
