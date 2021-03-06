﻿using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment.Builders
{
    public class LearnerEmploymentStatusBuilder: AbstractBuilder<LearnerEmploymentStatus>
    {
        public static DateTime DateEmpStat { get; } = new DateTime(2019, 8, 1);

        public static int? EmpId { get; } = 1234;

        public LearnerEmploymentStatusBuilder()
        {
            modelObject = new LearnerEmploymentStatus()
            {
                DateEmpStatApp = DateEmpStat,
                EmpId = EmpId
            };
        }
    }
}
