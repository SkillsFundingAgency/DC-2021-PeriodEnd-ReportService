using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR1920.DataStore.EF;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class MetaData
    {
        public DateTime DateGenerated { get; set; }

        public ReferenceDataVersion ReferenceDataVersions { get; set; }

        public IReadOnlyCollection<ValidationError> ValidationErrors { get; set; }

        public IReadOnlyCollection<ValidationRule> ValidationRules { get; set; }

        public IReadOnlyCollection<Lookup> Lookups { get; set; }

        public IlrCollectionDates CollectionDates { get; set; }
    }
}