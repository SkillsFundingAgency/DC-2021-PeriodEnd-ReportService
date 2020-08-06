using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model
{
    public class DataQualityProviderModel
    {
        public int CollectionId { get; set; }

        public ICollection<FilePeriodInfo> FileDetails { get; set; }

        public ICollection<RuleStats> RuleViolations { get; set; }

        public ICollection<ProviderSubmission> ProvidersWithoutValidLearners { get; set; }

        public ICollection<ProviderCount> ProvidersWithMostInvalidLearners { get; set; }

        public ICollection<Organisation> Organistions { get; set; }

        public ICollection<DataQualityModel> ReturningProviders { get; set; }

        public ICollection<ValidationRule> ValidationRules { get; set; }
    }
}
