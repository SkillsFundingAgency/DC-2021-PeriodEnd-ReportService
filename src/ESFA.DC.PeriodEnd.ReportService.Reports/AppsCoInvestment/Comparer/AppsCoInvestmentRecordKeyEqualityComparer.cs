using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment.Comparer
{
    public class AppsCoInvestmentRecordKeyEqualityComparer : IEqualityComparer<AppsCoInvestmentRecordKey>
    {
        public bool Equals(AppsCoInvestmentRecordKey x, AppsCoInvestmentRecordKey y)
        {
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(AppsCoInvestmentRecordKey obj)
        {
            return obj.GetHashCode();
        }
    }
}
