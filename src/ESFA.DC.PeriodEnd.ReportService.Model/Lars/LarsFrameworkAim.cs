using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.Lars
{
    public sealed class LarsFrameworkAim
    {
        public LarsFrameworkAim()
        {
        }

        public string LearnAimRef { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime EffectiveTo { get; set; }

        public int? FrameworkComponentType { get; set; }

        public int FworkCode { get; set; }

        public int ProgType { get; set; }

        public int PwayCode { get; set; }
    }
}
