using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment.Builders
{
    public class LearningDeliveriesBuilder : ILearningDeliveriesBuilder
    {
        public LearningDelivery GetLearningDeliveryForRecord(Learner learner, AppsCoInvestmentRecordKey record)
        {
            return learner?
                .LearningDeliveries
                .FirstOrDefault(ld => IlrLearningDeliveryRecordMatch(ld, record));
        }

        public bool IlrLearningDeliveryRecordMatch(LearningDelivery learningDelivery, AppsCoInvestmentRecordKey record)
        {
            return learningDelivery.ProgType == record.ProgrammeType
                   && learningDelivery.StdCode == record.StandardCode
                   && learningDelivery.FworkCode == record.FrameworkCode
                   && learningDelivery.PwayCode == record.PathwayCode
                   && learningDelivery.LearnStartDate == record.LearningStartDate
                   && learningDelivery.LearnAimRef.CaseInsensitiveEquals(record.LearningAimReference);
        }

        public bool HasLdm356Or361(LearningDelivery learningDelivery)
        {
            return learningDelivery?
                       .LearningDeliveryFams?
                       .Any(
                           fam =>
                               fam.Type.CaseInsensitiveEquals(LearnDelFamTypeConstants.LDM)
                               && (fam.Code == LearnDelFamCodeConstants.Code356 || fam.Code == LearnDelFamCodeConstants.Code361))
                   ?? false;
        }
    }
}
