using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ProviderSubmissions
{
    public sealed class OrganisationCollectionModel
    {
        public long Ukprn { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public bool Expected(DateTime periodStart, DateTime periodEnd)
        {
            if (End == null)
            {
                return true;
            }

            if (End > periodStart && (Start == null || Start < periodEnd))
            {
                return true;
            }

            return false;
        }
    }
}
