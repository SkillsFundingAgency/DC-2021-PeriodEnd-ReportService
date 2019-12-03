﻿using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer
{
    public interface ILLVPaymentRecordLRefOnlyKeyEqualityComparer
    {
        bool Equals(LearnerLevelViewPaymentsKey x, LearnerLevelViewPaymentsKey y);

        int GetHashCode(LearnerLevelViewPaymentsKey obj);
    }
}