using System.Collections.Generic;
using System.Linq;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class AppsMonthlyModelBuilder
    {
        public IEnumerable<AppsMonthlyRecord> Build(IEnumerable<Payment> payments, IEnumerable<Learner> learners)
        {
            return payments
                .GroupBy(
                    p =>
                        new RecordKey(p.LearnerReferenceNumber,
                            p.LearnerUln,
                            p.LearningAimReference,
                            p.LearningStartDate,
                            p.LearningAimProgrammeType,
                            p.LearningAimStandardCode,
                            p.LearningAimFrameworkCode,
                            p.LearningAimPathwayCode,
                            p.ReportingAimFundingLineType,
                            p.PriceEpisodeIdentifier))
                .Select(k =>
                {
                    var learner = GetLearnerForRecord(learners, k.Key);
                    var learningDelivery = GetLearnerLearningDeliveryForRecord(learner, k.Key);

                    return new AppsMonthlyRecord()
                    {
                        RecordKey = k.Key,
                        Learner = learner,
                        LearningDelivery = learningDelivery,

                    };
                });
        }

        public Learner GetLearnerForRecord(IEnumerable<Learner> learners, RecordKey recordKey)
        {
            return learners.FirstOrDefault(l => l.LearnRefNumber.CaseInsensitiveEquals(recordKey.LearnerReferenceNumber));
        }

        public LearningDelivery GetLearnerLearningDeliveryForRecord(Learner learner, RecordKey recordKey)
        {
            // BR2 - UKPRN and LearnRefNumber is implicitly matched, not included on models
            return learner?
                .LearningDeliveries?
                .FirstOrDefault(ld =>
                    ld.ProgType == recordKey.ProgrammeType
                    && ld.StdCode == recordKey.StandardCode
                    && ld.FworkCode == recordKey.FrameworkCode
                    && ld.PwayCode == recordKey.PathwayCode
                    && ld.LearnStartDate == recordKey.LearnStartDate
                    && ld.LearnAimRef.CaseInsensitiveEquals(recordKey.LearningAimReference));
        }
    }
}
