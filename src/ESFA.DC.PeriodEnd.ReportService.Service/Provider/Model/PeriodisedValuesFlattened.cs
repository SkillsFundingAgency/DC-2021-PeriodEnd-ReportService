namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider.Model
{
    public class PeriodisedValuesFlattened
    {
        public string AttributeName { get; set; }

        public string FundLine { get; set; }

        public decimal?[] Periods { get; set; }
    }
}
