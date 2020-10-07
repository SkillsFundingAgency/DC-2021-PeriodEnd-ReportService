using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.Comparer
{
    public class LearningDeliveryEarningComparer : IEqualityComparer<LearningDeliveryEarning>
    {
        public bool Equals(LearningDeliveryEarning x, LearningDeliveryEarning y)
        {
            return (x.LearnRefNumber == y.LearnRefNumber);
        }

        public int GetHashCode(LearningDeliveryEarning obj)
        {
            return obj.GetHashCode();
        }
    }
}
