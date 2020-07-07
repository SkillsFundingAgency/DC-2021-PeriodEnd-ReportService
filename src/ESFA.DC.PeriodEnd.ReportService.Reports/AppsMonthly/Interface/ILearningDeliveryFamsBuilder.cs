using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface
{
    public interface ILearningDeliveryFamsBuilder
    {
        LearningDeliveryFams BuildLearningDeliveryFamsForLearningDelivery(LearningDelivery learningDelivery);
    }
}
