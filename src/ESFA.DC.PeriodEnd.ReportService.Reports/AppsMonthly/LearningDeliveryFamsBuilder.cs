using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class LearningDeliveryFamsBuilder : ILearningDeliveryFamsBuilder
    {
        public LearningDeliveryFams BuildLearningDeliveryFamsForLearningDelivery(LearningDelivery learningDelivery)
        {
            if (learningDelivery?.LearningDeliveryFams != null)
            {
                var ldmFams = GetLearnDelFamCodesOfType(learningDelivery.LearningDeliveryFams, LearnDelFamTypeConstants.LDM, 6);

                return new LearningDeliveryFams()
                {
                    LDM1 = ldmFams[0],
                    LDM2 = ldmFams[1],
                    LDM3 = ldmFams[2],
                    LDM4 = ldmFams[3],
                    LDM5 = ldmFams[4],
                    LDM6 = ldmFams[5],
                };
            }

            return null;
        }

        private string[] GetLearnDelFamCodesOfType(IEnumerable<LearningDeliveryFam> learningDeliveryFams, string type, int count)
        {
            return learningDeliveryFams
                .Where(f => f.Type.CaseInsensitiveEquals(type))
                .Select(f => f.Code)
                .ToFixedLengthArray(count);
        }
    }
}
