using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.Logging.Interfaces;
using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.DateTimeProvider.Interface;
using System.Threading;
using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.PeriodEndMetrics;
using FluentAssertions;
using System.Linq;
using System.IO;
using Aspose.Cells;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Service;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Tests.Reports
{
    public sealed class TestPeriodEndMetricsReport
    {
        private const string EarningsPaymentsTabName = "Earnings vs Payments";

        [Fact]
        public async Task TestPeriodEndMetricsReportGeneration()
        {
            Stream csv = new MemoryStream();
            DateTime dateTime = DateTime.UtcNow;
            string reportFileName = "Period End Metrics R";
            int ukPrn = 10036143;
            int collectionYear = 1920;
            int returnPeriod = 2;
            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(ukPrn);
            reportServiceContextMock.SetupGet(x => x.CollectionYear).Returns(collectionYear);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(returnPeriod);

            var filename = $"{returnPeriod.ToString().PadLeft(2, '0')}_{reportFileName} {dateTime:yyyyMMdd-HHmmss}";

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IPaymentsService> paymentServiceMock = new Mock<IPaymentsService>();
            Mock<IPeriodEndQueryService1920> periodEndQueryServiceMock = new Mock<IPeriodEndQueryService1920>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            IValueProvider valueProvider = new ValueProvider();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            
            storage.Setup(x => x.SaveAsync($"{filename}.xlsx", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<string, Stream, CancellationToken>((key, value, ct) => csv = value)
                .Returns(Task.CompletedTask);

            var paymentInfo = BuildPaymentModel(collectionYear, returnPeriod);
            var periodEndInfo = BuildPeriodEndModel(returnPeriod);
            
            paymentServiceMock.Setup(x => x.GetPaymentMetrics(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(paymentInfo);
            periodEndQueryServiceMock.Setup(x => x.GetPeriodEndMetrics(It.IsAny<int>()))
                .ReturnsAsync(periodEndInfo);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);

            var report = new PeriodEndMetricsReport(
                logger: logger.Object,
                paymentsService: paymentServiceMock.Object,
                ilrPeriodEndService: periodEndQueryServiceMock.Object,
                dateTimeProvider: dateTimeProviderMock.Object,
                persistenceService: storage.Object,
                valueProvider: valueProvider);

            await report.GenerateReport(reportServiceContextMock.Object, CancellationToken.None);

            Workbook wb = new Workbook(csv);
            wb.Should().NotBeNull();
            wb.Worksheets.Count().Should().BeGreaterThan(0);
        }

        private IEnumerable<PaymentMetrics> BuildPaymentModel(int collectionYear, int returnPeriod)
        {
            return new List<PaymentMetrics>()
            {
                new PaymentMetrics()
                {
                    TransactionType = 1,
                    EarningsYTD = 1.25M,
                    EarningsACT1 = 1.50M,
                    EarningsACT2 = 1.75M,
                    NegativeEarnings = 1.0M,
                    NegativeEarningsACT1 = 2.0M,
                    NegativeEarningsACT2 = 2.25M,
                    PaymentsYTD = 2.75M,
                    PaymentsACT1 = 3.0M,
                    PaymentsACT2 = 3.25M,
                    DataLockErrors = 1.0M,
                    HeldBackCompletion = 5.0M,
                    HBCPACT1 = 0.5M,
                    HBCPACT2 = 0.75M
                },
                new PaymentMetrics()
                {
                    TransactionType = 2,
                    EarningsYTD = 1.25M,
                    EarningsACT1 = 1.50M,
                    EarningsACT2 = 1.75M,
                    NegativeEarnings = 1.0M,
                    NegativeEarningsACT1 = 2.0M,
                    NegativeEarningsACT2 = 2.25M,
                    PaymentsYTD = 2.75M,
                    PaymentsACT1 = 3.0M,
                    PaymentsACT2 = 3.25M,
                    DataLockErrors = 1.0M,
                    HeldBackCompletion = 5.0M,
                    HBCPACT1 = 0.5M,
                    HBCPACT2 = 0.75M
                }
            };
        }

        private IEnumerable<IlrMetrics> BuildPeriodEndModel(int returnPeriod)
        {
            return new List<IlrMetrics>()
            {
                new IlrMetrics()
                    {
                        TransactionType = "9",
                        EarningsYTD = 125.50M,
                        EarningsACT1 = 120.75M,
                        EarningsACT2 = 150.25M
                }
            };
        }
    }
}
