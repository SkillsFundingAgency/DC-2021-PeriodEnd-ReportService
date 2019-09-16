using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.Lars
{
    public sealed class LearnerAndDeliveries
    {
        public LearnerAndDeliveries(string learnerLearnRefNumber, List<LearningDelivery> learningDeliveries)
        {
            LearnerLearnRefNumber = learnerLearnRefNumber;
            LearningDeliveries = learningDeliveries;
        }

        public string LearnerLearnRefNumber { get; }

        public List<LearningDelivery> LearningDeliveries { get; }
    }
}
