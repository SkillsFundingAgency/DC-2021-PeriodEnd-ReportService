using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class ReferenceDataRoot
    {
        public ReferenceDataRoot() { }

        public MetaData.MetaData MetaDatas { get; set; }
        public IReadOnlyCollection<ApprenticeshipEarningsHistory> AppsEarningsHistories { get; set; }
        public IReadOnlyCollection<EasFundingLine> EasFundingLines { get; set; }
        public IReadOnlyCollection<Employer> Employers { get; set; }
        public IReadOnlyCollection<EPAOrganisation> EPAOrganisations { get; set; }
        public IReadOnlyCollection<FcsContractAllocation> FCSContractAllocations { get; set; }
        public IReadOnlyCollection<LARSLearningDelivery> LARSLearningDeliveries { get; set; }
        public IReadOnlyCollection<LARSStandard> LARSStandards { get; set; }
        public IReadOnlyCollection<Organisation> Organisations { get; set; }
        public IReadOnlyCollection<Postcode> Postcodes { get; set; }
        public DevolvedPostcodes DevolvedPostocdes { get; set; }
        public IReadOnlyCollection<long> ULNs { get; set; }
    }
}