using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment.Builders
{
    public class AecPriceEpisodePeriodisedValueBuilder : AbstractBuilder<AECApprenticeshipPriceEpisodePeriodisedValues>
    {
        public const string LearnRefNumber = "LearnRefNumber";

        public const int AimSeqNumber = 1;

        public const string PriceEpisodeIdentifier = "priceepisodeidentifier";

        public const string AttributeName = "AttributeName";

        public const decimal Period1 = 0;

        public const decimal Period2 = 0;
               
        public const decimal Period3 = 0;
               
        public const decimal Period4 = 0;

        public const decimal Period5 = 0;
               
        public const decimal Period6 = 0;
                
        public const decimal Period7 = 0;
               
        public const decimal Period8 = 0;
               
        public const decimal Period9 = 0;
               
        public const decimal Period10 = 0;
                 
        public const decimal Period11 = 0;
               
        public const decimal Period12 = 0;

        public AecPriceEpisodePeriodisedValueBuilder()
        {
            modelObject = new AECApprenticeshipPriceEpisodePeriodisedValues()
            {
                AimSeqNumber = AimSeqNumber,
                LearnRefNumber = LearnRefNumber,
                AttributeName = AttributeName,
                Period1 = Period1,
                Period2 = Period2,
                Period3 = Period3,
                Period4 = Period4,
                Period5 = Period5,
                Period6 = Period6,
                Period7 = Period7,
                Period8 = Period8,
                Period9 = Period9,
                Period10 = Period10,
                Period11 = Period11,
                Period12 = Period12
            };

        }
    }
}
