using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders
{
    public class AecLearningDeliveryBuilder : AbstractBuilder<AecLearningDelivery>
    {
        public const int PlannedNumOnProgInstalm = 2;

        public AecLearningDeliveryBuilder()
        {
            modelObject = new AecLearningDelivery()
            {
                PlannedNumOnProgInstalm = PlannedNumOnProgInstalm
            };
        }
    }
}
