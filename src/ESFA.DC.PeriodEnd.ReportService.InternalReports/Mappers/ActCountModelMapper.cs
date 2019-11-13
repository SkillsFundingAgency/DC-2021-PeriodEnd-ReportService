using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ActCountReport;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Mappers
{
    public sealed class ActCountModelMapper : ClassMap<ActCountModel>
    {
        public ActCountModelMapper()
        {
            Map(m => m.Ukprn).Index(0).Name("UKPRN");
            Map(m => m.ActCountOne).Index(1).Name("Number of Learners with ACT1");
            Map(m => m.ActCountTwo).Index(2).Name("Number of Learners with ACT2");
        }
    }
}
