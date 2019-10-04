using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.ReportModels
{
    public sealed class CollectionStatsModel
    {
        public string CollectionName { get; set; }

        public int CountOfComplete { get; set; }

        public int CountOfFail { get; set; }

        public int Total => CountOfComplete + CountOfFail;

        public double Percent
        {
            get
            {
                if (CountOfComplete == 0 || CountOfFail == 0)
                {
                    return 0;
                }

                return Math.Round(((double)CountOfComplete / (CountOfComplete + CountOfFail) * 100), 2);
            }
        }
    }
}
