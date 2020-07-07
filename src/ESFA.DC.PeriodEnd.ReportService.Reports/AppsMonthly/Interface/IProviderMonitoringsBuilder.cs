using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface
{
    public interface IProviderMonitoringsBuilder
    {
        ProviderMonitorings BuildProviderMonitorings(Learner learner, LearningDelivery learningDelivery);
    }
}
