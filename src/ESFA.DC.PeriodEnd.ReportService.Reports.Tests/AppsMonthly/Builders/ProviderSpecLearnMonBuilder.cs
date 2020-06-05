using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders
{
    public class ProviderSpecLearnMonBuilder : AbstractBuilder<ProviderSpecLearnMon>
    {
        public const string ProviderSpecLearnMonOccur = "A";
        public const string ProviderSpecLearnMon = "ProviderSpecLearnMon";

        public ProviderSpecLearnMonBuilder()
        {
            modelObject = new ProviderSpecLearnMon()
            {
                ProvSpecLearnMonOccur = ProviderSpecLearnMonOccur,
                ProvSpecLearnMon = ProviderSpecLearnMon,
            };
        }
    }
}
