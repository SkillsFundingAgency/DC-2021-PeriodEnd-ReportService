﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.DataProvider
{
    public interface ILearnerDataProvider
    {
        Task<ICollection<Learner>> GetLearnersAsync(int ukprn, CancellationToken cancellationToken);
    }
}
