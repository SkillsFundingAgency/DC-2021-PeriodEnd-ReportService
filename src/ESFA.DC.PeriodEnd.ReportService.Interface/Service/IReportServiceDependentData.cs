using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IReportServiceDependentData
    {
        T Get<T>();

        void Set(Type type, object value);
    }
}
