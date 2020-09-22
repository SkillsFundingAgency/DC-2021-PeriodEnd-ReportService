using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.Comparer
{
    public class DataLockComparer : IEqualityComparer<DataLock>
    {
        public bool Equals(DataLock x, DataLock y)
        {
            return (x.LearnerReferenceNumber == y.LearnerReferenceNumber && x.CollectionPeriod == y.CollectionPeriod);
        }

        public int GetHashCode(DataLock obj)
        {
            return obj.GetHashCode();
        }
    }
}
