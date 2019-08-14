using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IFcsProviderService
    {
        Task<AppsMonthlyPaymentFcsInfo> GetFcsInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        ///// <summary>
        ///// Returns the ContractAllocationNumber for the given UKPRN and FundingStreamPeriodCode.
        ///// </summary>
        ///// <param name="ukPrn">The UKPRN of the required Contractor. </param>
        ///// <param name="fundingStreamPeriodCode">The FunsingStreamPeriodCode of the required ContractAllocation. </param>
        ///// <returns>A <see cref="string"/> contractAllocationNumber. </returns>
        ////string GetFcsContractAllocationNumber(int ukPrn, string fundingStreamPeriodCode);
    }
}
