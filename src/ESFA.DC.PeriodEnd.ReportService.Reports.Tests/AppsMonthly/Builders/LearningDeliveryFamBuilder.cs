using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders
{
    public class LearningDeliveryFamBuilder : AbstractBuilder<LearningDeliveryFam>
    {
        public const string Type = "LDM";
        public const string Code = "Code";

        public LearningDeliveryFamBuilder()
        {
            modelObject = new LearningDeliveryFam()
            {
                Type = Type,
                Code = Code
            };
        }
    }
}
