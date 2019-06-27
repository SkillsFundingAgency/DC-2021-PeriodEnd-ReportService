using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.Lars
{
    public sealed class LearnerAndDeliveries
    {
        public string LearnerLearnRefNumber { get; }

        public List<LearningDelivery> LearningDeliveries { get; }

        public LearnerAndDeliveries(string learnerLearnRefNumber, List<LearningDelivery> learningDeliveries)
        {
            LearnerLearnRefNumber = learnerLearnRefNumber;
            LearningDeliveries = learningDeliveries;
        }
    }
}
