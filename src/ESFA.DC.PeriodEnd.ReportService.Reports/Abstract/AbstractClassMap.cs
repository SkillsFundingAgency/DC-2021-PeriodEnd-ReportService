using System;
using System.Linq.Expressions;
using CsvHelper.Configuration;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Abstract
{
    public abstract class AbstractClassMap<T> : ClassMap<T>
    {
        private int _index;
        
        protected MemberMap<T, TOut> MapIndex<TOut>(Expression<Func<T, TOut>> expression) => Map(expression).Index(_index++);

        protected MemberMap<object, object> MapIndex() => Map().Index(_index++);
    }
}
