using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.ReportModels
{
    public sealed class CollectionStatsModel
    {
        public string CollectionName { get; set; }

        public int CountOfComplete { get; set; }

        public int CountOfFail { get; set; }

        public int Total => CountOfComplete + CountOfFail;

        public int Percent
        {
            get
            {
                if (CountOfComplete == 0 || CountOfFail == 0)
                {
                    return 0;
                }

                var value = ((double)CountOfComplete / (CountOfComplete + CountOfFail)) * 100;
                return Convert.ToInt32(Math.Round(value, 0));
            }
        }
    }
}
