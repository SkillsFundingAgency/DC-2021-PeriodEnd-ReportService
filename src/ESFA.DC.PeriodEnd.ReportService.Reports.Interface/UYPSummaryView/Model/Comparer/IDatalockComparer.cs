using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.Comparer
{
    public interface IDataLockComparer
    {
        bool Equals(DataLock x, DataLock y);

        int GetHashCode(DataLock obj);
    }
}
