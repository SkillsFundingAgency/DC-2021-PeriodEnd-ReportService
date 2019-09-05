using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Mappers
{
    public class DataExtractMapper : ClassMap<DataExtractModel>
    {
        public DataExtractMapper()
        {
            int i = 0;
            Map(m => m.Id).Index(i++).Name("Id");
            Map(m => m.CollectionReturnCode).Index(i++).Name("CollectionReturnCode");
            Map(m => m.UkPrn).Index(i++).Name("ukprn");
            Map(m => m.OrganisationId).Index(i++).Name("OrganisationId");
            Map(m => m.PeriodTypeCode).Index(i++).Name("PeriodTypeCode");
            Map(m => m.Period).Index(i++).Name("Period");
            Map(m => m.FundingStreamPeriodCode).Index(i++).Name("FundingStreamPeriodCode");
            Map(m => m.CollectionType).Index(i++).Name("CollectionType");
            Map(m => m.ContractAllocationNumber).Index(i++).Name("ContractAllocationNumber");
            Map(m => m.UoPCode).Index(i++).Name("UoPCode");
            Map(m => m.DeliverableCode).Index(i++).Name("DeliverableCode");
            Map(m => m.ActualVolume).Index(i++).Name("ActualVolume");
            Map(m => m.ActualValue).Index(i++).Name("ActualValue");
        }
    }
}
