namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Constants
{
    public static class JobQueue
    {
        public static class CollectionTypes
        {
            public const string EAS = "EAS";
            public const string ESF = "ESF";
            public const string ILR = "ILR";
        }

        public static class Status
        {
            public const int Completed = 4;
            public const int FailedRetry = 5;
            public const int Failed = 6;
        }
    }
}
