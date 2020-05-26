using System;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.EAS2021.EF;
using ESFA.DC.ILR2021.DataStore.EF;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Provider;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Tests.Providers
{
    public class PeriodisedValueLookupProviderTests
    {
        private const string ConnectionString = "";

        [Fact]
        public async Task BuildFm35Dictionary()
        {
            var result = await NewService().BuildFm35DictionaryAsync(10005788, CancellationToken.None);
        }

        [Fact]
        public async Task BuildFm25Dictionary()
        {
            var result = await NewService().BuildFm25DictionaryAsync(10005788, CancellationToken.None);
        }

        [Fact]
        public async Task BuildAlbDictionary()
        {
            var result = await NewService().BuildFm99DictionaryAsync(10005788, CancellationToken.None);
        }

        [Fact]
        public async Task BuildTblDictionary()
        {
            var result = await NewService().BuildFm81DictionaryAsync(10005788, CancellationToken.None);
        }

        [Fact]
        public async Task ProvideAsync()
        {
            var context = new Mock<IReportServiceContext>();

            context.SetupGet(c => c.Ukprn).Returns(10005788);

            var result = await NewService().ProvideAsync(context.Object, CancellationToken.None);
        }

        public static PeriodisedValuesLookupProviderService NewService()
        {
            return new PeriodisedValuesLookupProviderService(
                () => new ILR2021_DataStoreEntities(NewBuilder<ILR2021_DataStoreEntities>().Options),
                () => new EasContext(NewBuilder<EasContext>().Options),
                () => new DASPaymentsContext(NewBuilder<DASPaymentsContext>().Options));
        }

        private static DbContextOptionsBuilder<T> NewBuilder<T>()
            where T : DbContext
        {
            var builder = new DbContextOptionsBuilder<T>();
            //builder.UseSqlServer(ConnectionString);
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            return builder;
        }
    }
}
