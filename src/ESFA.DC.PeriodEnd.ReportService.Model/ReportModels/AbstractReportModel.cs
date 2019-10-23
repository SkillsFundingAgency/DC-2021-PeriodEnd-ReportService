using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.ReportModels
{
    public class AbstractReportModel
    {
        public static int? ReturnPeriodSetter;

        public int? ReturnPeriod => ReturnPeriodSetter;

        public virtual string OfficialSensitive { get; set; }
    }
}
