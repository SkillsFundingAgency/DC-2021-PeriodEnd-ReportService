using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;

namespace ESFA.DC.PeriodEnd.ReportService.Stateless.Configuration
{
    public sealed class VersionInfo : IVersionInfo
    {
        public string ServiceReleaseVersion { get; set; }
    }
}
