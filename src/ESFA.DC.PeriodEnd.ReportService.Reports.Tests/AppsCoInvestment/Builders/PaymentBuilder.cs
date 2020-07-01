using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment.Builders
{
    public class PaymentBuilder : AbstractBuilder<Payment>
    {
        public const short AcademicYear = 1920; 
        public const decimal Amount = 10.0m;
        public const long ApprenticeshipId = 1;
        public const byte CollectionPeriod = 1;
        public const byte ContractType = 1;
        public const byte DeliveryPeriod = 1;
        public const byte FundingSource = 3;
        public const string LearnerReferenceNumber = "fm36 17 23";
        public const long LearnerUln = 1213456789;
        public const int FrameworkCode = 10;
        public const int PathwayCode = 20;
        public const int ProgrammeType = 30;
        public const string LearningAimReference = "ZPROG001";
        public const int StandardCode = 40;
        public static DateTime LearningStartDate = new DateTime(2020, 8, 1);
        public const string LegalEntityName = "LegalEntityName";
        public const string PriceEpisodeIdentifier = "25-1-01/08/2019";
        public const decimal SfaContributionPercentage = 0.90000m;
        public const byte TransactionType = 1;

        public PaymentBuilder()
        {
            modelObject = new Payment()
            {
                AcademicYear = AcademicYear,
                Amount = Amount,
                ApprenticeshipId = ApprenticeshipId,
                CollectionPeriod = CollectionPeriod,
                ContractType = ContractType,
                DeliveryPeriod = DeliveryPeriod,
                FundingSource = FundingSource,
                LearnerReferenceNumber = LearnerReferenceNumber,
                LearnerUln = LearnerUln,
                LearningAimFrameworkCode = FrameworkCode,
                LearningAimPathwayCode = PathwayCode,
                LearningAimProgrammeType = ProgrammeType,
                LearningAimReference = LearningAimReference,
                LearningAimStandardCode = StandardCode,
                LearningStartDate = LearningStartDate,
                LegalEntityName = LegalEntityName,
                PriceEpisodeIdentifier = PriceEpisodeIdentifier,
                SfaContributionPercentage = SfaContributionPercentage,
                TransactionType = TransactionType
            };
        }
    }
}
