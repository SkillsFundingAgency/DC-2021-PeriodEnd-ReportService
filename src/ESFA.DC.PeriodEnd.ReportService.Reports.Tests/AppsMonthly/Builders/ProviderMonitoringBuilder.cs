using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders
{
    public class ProviderMonitoringBuilder : AbstractBuilder<ProviderMonitoring>
    {
        public const string Occur = "A";
        public const string Mon = "Mon";

        public ProviderMonitoringBuilder()
        {
            modelObject = new ProviderMonitoring()
            {
                Occur = Occur,
                Mon = Mon,
            };
        }
    }
}
