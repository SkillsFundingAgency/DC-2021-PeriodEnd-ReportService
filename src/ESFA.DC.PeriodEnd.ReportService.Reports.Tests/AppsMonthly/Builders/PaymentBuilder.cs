using System;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class PaymentBuilder : AbstractBuilder<Payment>
    {
        public const string LearnerReferenceNumber = "LearnRefNumber";
        public const long LearnerUln = 123456789;
        public const int FrameworkCode = 10;
        public const int PathwayCode = 20;
        public const int ProgrammeType = 30;
        public const string LearningAimReference = "LearnAimRef";
        public const int StandardCode = 40;
        public static DateTime LearningStartDate = new DateTime(2020, 8, 1);
        public const string PriceEpisodeIdentifier = "PriceEpisodeIdentifier";
        public const string ReportingAimFundingLineType = "ReportingAimFundingLineType";
        public static Guid EarningEventId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        public const byte CollectionPeriod = 1;
        public const byte DeliveryPeriod = 1;

        public PaymentBuilder()
        {
            modelObject = new Payment()
            {
                LearnerReferenceNumber = LearnerReferenceNumber,
                LearnerUln = LearnerUln,
                LearningAimFrameworkCode = FrameworkCode,
                LearningAimPathwayCode = PathwayCode,
                LearningAimProgrammeType = ProgrammeType,
                LearningAimReference = LearningAimReference,
                LearningAimStandardCode = StandardCode,
                LearningStartDate = LearningStartDate,
                PriceEpisodeIdentifier = PriceEpisodeIdentifier,
                ReportingAimFundingLineType = ReportingAimFundingLineType,
                EarningEventId = EarningEventId,
                CollectionPeriod = CollectionPeriod,
                DeliveryPeriod = DeliveryPeriod,
            };
        }
    }
}
