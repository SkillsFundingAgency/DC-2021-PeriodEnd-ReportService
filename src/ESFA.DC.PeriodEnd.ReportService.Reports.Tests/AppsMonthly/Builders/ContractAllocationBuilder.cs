using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders
{
    public class ContractAllocationBuilder : AbstractBuilder<ContractAllocation>
    {
        public ContractAllocationBuilder()
        {
            modelObject = new ContractAllocation()
            {
                ContractAllocationNumber = "ContractAllocationNumber",
                FundingStreamPeriod = "FundingStreamPeriod",
            };
        }
    }
}
