using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.FundingSummary.Builders
{
    public class AbstractBuilder<T>
    {
        protected T modelObject;

        public AbstractBuilder<T> With<TProperty>(Expression<Func<T, TProperty>> setter, TProperty value)
        {
            var memberExpression = (MemberExpression)setter.Body;
            var property = (PropertyInfo)memberExpression.Member;

            property.SetValue(modelObject, value);

            return this;
        }

        public T Build()
        {
            return modelObject;
        }
    }
}
