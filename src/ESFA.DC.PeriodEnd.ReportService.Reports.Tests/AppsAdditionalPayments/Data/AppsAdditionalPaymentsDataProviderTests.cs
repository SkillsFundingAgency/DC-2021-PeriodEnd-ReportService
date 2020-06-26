using System.Data.SqlClient;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments.Das;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments.Ilr;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.DataProvider;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsAdditionalPayments.Data
{
    public class AppsAdditionalPaymentsDataProviderTests
    {
        //[Fact(Skip = "Test against real Db, not to run in CI/CD")]
        [Fact]
        public async void TestAgainstRealDb()
        {
            var ilrConnectionString = 
                "data source=DCOL-TST-SqlServer-WEU.database.windows.net;initial catalog=ILR1920DataStore;User Id=ILR1920DataStore_RO_User;Password=B42k7A@Hjp1lZodbahuDVtiKnRxrGLy-FEIX6OsSqTUz59w8PWYgQN03ecCMmfJverE-xdtNwIB;MultipleActiveResultSets=true;Encrypt=True;";
            var dasConnectionString =
                "Data Source=DCOL-TST-SqlServer-WEU.database.windows.net;Initial Catalog=DASPayments;User Id=DASPaymentROUser;Password=X2Bk1WjYqCuHsley83Z@JfUcxRPItModLagz6mKE9DvVApSO04F-G5Q7rnihTwNb2Ejc56hOigx;Encrypt=True;";

            var collectionYear = 1920;
            var ukprn = 10000055;

            SqlConnection ilrSqlFunc() => new SqlConnection(ilrConnectionString);
            SqlConnection dasSqlFunc() => new SqlConnection(dasConnectionString);

            var paymentsDataProvider = new PaymentsDataProvider(dasSqlFunc) as IPaymentsDataProvider;
            var learnerDataProvider = new LearnerDataProvider(ilrSqlFunc) as ILearnerDataProvider;
            var aecLearningDeliveryDataProvider = new AecLearningDeliveryDataProvider(ilrSqlFunc) as IAecLearningDeliveryDataProvider;
            var appsPriceEpisodePeriodisedValuesDataProvider = new AppsPriceEpisodePeriodisedValuesDataProvider(ilrSqlFunc) as IAppsPriceEpisodePeriodisedValuesDataProvider;

            var dataProvider = new AppsAdditionalPaymentsDataProvider(
                paymentsDataProvider, 
                learnerDataProvider, 
                aecLearningDeliveryDataProvider, 
                appsPriceEpisodePeriodisedValuesDataProvider) as IAppsAdditionalPaymentsDataProvider;

            var cancellationToken = new CancellationToken();
            var reportServiceContext = new Mock<IReportServiceContext>();
            reportServiceContext.Setup(rsc => rsc.CollectionYear).Returns(collectionYear);
            reportServiceContext.Setup(rsc => rsc.Ukprn).Returns(ukprn);

            var payments = await dataProvider.GetPaymentsAsync(reportServiceContext.Object, cancellationToken);
            payments.Should().NotBeNull();
            payments.Should().NotBeEmpty();

            var learners = await dataProvider.GetLearnersAsync(reportServiceContext.Object, cancellationToken);
            learners.Should().NotBeNull();
            learners.Should().NotBeEmpty();

            var aecLearningDeliveries = await dataProvider.GetAecLearningDeliveriesAsync(reportServiceContext.Object,
                cancellationToken);
            aecLearningDeliveries.Should().NotBeNull();
            aecLearningDeliveries.Should().NotBeEmpty();

            var appPriceEpisodePeriodisedValues = await dataProvider.GetPriceEpisodesAsync(reportServiceContext.Object, cancellationToken);
            appPriceEpisodePeriodisedValues.Should().NotBeNull();
            appPriceEpisodePeriodisedValues.Should().NotBeEmpty();

            var paymentFundingLineFormatter = new PaymentLineFormatter() as IPaymentLineFormatter;
            var earningsAndPaymentsBuilder = new EarningsAndPaymentsBuilder() as IEarningsAndPaymentsBuilder;

            var appsAdditionalPaymentsModelBuilder =
                new AppsAdditionalPaymentsModelBuilder(paymentFundingLineFormatter, earningsAndPaymentsBuilder) as IAppsAdditionalPaymentsModelBuilder;

            var results = appsAdditionalPaymentsModelBuilder.Build(payments, learners, aecLearningDeliveries, appPriceEpisodePeriodisedValues);

            results.Should().NotBeNull();
        }
    }
}