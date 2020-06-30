using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders
{
    public interface ILearningDeliveriesBuilder
    {
        LearningDelivery GetLearningDeliveryForRecord(Learner learner, AppsCoInvestmentRecordKey record);

        bool HasLdm356Or361(LearningDelivery learningDelivery);
    }
}
