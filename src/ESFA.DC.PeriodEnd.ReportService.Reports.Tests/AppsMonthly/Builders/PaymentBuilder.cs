using System;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class PaymentBuilder : AbstractBuilder<Payment>
    {
        public PaymentBuilder()
        {
            modelObject = new Payment()
            {
                LearnerReferenceNumber = "LearnRefNumber",
                LearnerUln = 123456789,
                LearningAimFrameworkCode = 10,
                LearningAimPathwayCode = 20,
                LearningAimProgrammeType = 30,
                LearningAimReference = "LearnAimRef",
                LearningAimStandardCode = 40,
                LearningStartDate = new DateTime(2020, 8, 1),
                PriceEpisodeIdentifier = "PriceEpisodeIdentifier",
                ReportingAimFundingLineType = "ReportingAimFundingLineType"
            };
        }
    }
}
