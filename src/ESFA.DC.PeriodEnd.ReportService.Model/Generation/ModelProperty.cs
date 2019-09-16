using System.Reflection;

namespace ESFA.DC.PeriodEnd.ReportService.Model.Generation
{
    public sealed class ModelProperty
    {
        public ModelProperty(string[] names, PropertyInfo methodInfo)
        {
            Names = names;
            MethodInfo = methodInfo;
        }

        public string[] Names { get; }

        public PropertyInfo MethodInfo { get; }
    }
}
