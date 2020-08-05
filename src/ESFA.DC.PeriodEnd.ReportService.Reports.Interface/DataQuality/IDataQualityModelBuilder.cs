using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality
{
    public interface IDataQualityModelBuilder
    {
        DataQualityProviderModel Build(DataQualityProviderModel providerModel);
    }
}
