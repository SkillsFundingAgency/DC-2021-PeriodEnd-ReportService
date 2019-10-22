namespace ESFA.DC.PeriodEnd.ReportService.Service.Constants
{
    public static class DASPayments
    {
        public static class FundingSource
        {
            public const byte Levy = 1;
            public const byte Co_Invested_SFA = 2;
            public const byte Co_Invested_Employer = 3;
            public const byte Fully_Funded_SFA = 4;
            public const byte LevyTransfer = 5;
        }

        public static class TransactionType
        {
            public const int Learning_On_Programme = 1;
            public const int Completion = 2;
            public const int Balancing = 3;
            public const int First_16To18_Employer_Incentive = 4;
            public const int First_16To18_Provider_Incentive = 5;
            public const int Second_16To18_Employer_Incentive = 6;
            public const int Second_16To18_Provider_Incentive = 7;
            public const int On_Programme_16To18_Framework_Uplift = 8;
            public const int Completion_16To18_Framework_Uplift = 9;
            public const int Balancing_16To18_Framework_Uplift = 10;
            public const int First_Disadvantage_Payment = 11;
            public const int Second_Disadvantage_Payment = 12;
            public const int On_Programme_Maths_and_English = 13;
            public const int BalancingMathAndEnglish = 14;
            public const int Learning_Support = 15;
            public const int Apprenticeship = 16;

            public static int[] All = new[]
            {
                Learning_On_Programme,
                Completion,
                Balancing,
                First_16To18_Employer_Incentive,
                First_16To18_Provider_Incentive,
                Second_16To18_Employer_Incentive,
                Second_16To18_Provider_Incentive,
                On_Programme_16To18_Framework_Uplift,
                Completion_16To18_Framework_Uplift,
                Balancing_16To18_Framework_Uplift,
                First_Disadvantage_Payment,
                Second_Disadvantage_Payment,
                On_Programme_Maths_and_English,
                BalancingMathAndEnglish,
                Learning_Support,
                Apprenticeship,
            };
        }

        public static class PaymentStatus
        {
            public const int Pending_Approval = 0;
            public const int Active = 1;
            public const int Paused = 2;
            public const int Cancelled = 3;
            public const int Completed = 4;
            public const int Deleted = 5;
        }
    }
}
