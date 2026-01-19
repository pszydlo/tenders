using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Tenders.Application.Exceptions;
using Tenders.Domain.Models;
using Tenders.Infrastucture.Caching;
using Tenders.Infrastucture.Options;
using Tenders.Infrastucture.Persistence;
using Tenders.Infrastucture.TendersApi;
using Tenders.Infrastucture.TendersApi.Models;

namespace Tenders.InfrastuctureTests.Caching;

[TestFixture]
public class TendersIndexProviderTests
{
    [Test]
    public async Task GetIndexAsync_ShouldThrowServiceUnavailable_WhenCacheNotBuilt()
    {
        var sut = CreateSut(
            options: new TendersOptions { BaseUrl = "https://example.test", MaxSourcePages = 1 },
            apiClient: new Mock<ITendersApiClient>().Object,
            pagesStore: new Mock<ITendersPagesStore>().Object,
            cache: new MemoryCache(new MemoryCacheOptions()));

        Func<Task> act = () => sut.GetIndexAsync(CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ServiceUnavailableException>();
        ex.Which.RetryAfterSeconds.Should().Be(30);
    }

    [Test]
    public async Task TryWarmUpFromDiskAsync_ShouldReturnFalse_WhenNoCachedPagesExist()
    {
        var pagesStore = new Mock<ITendersPagesStore>();
        pagesStore
            .Setup(s => s.TryReadAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TendersGuruPagedResponse<TenderListItemApiModel>?)null);

        var cache = new MemoryCache(new MemoryCacheOptions());

        var sut = CreateSut(
            options: new TendersOptions { BaseUrl = "https://example.test", MaxSourcePages = 3 },
            apiClient: new Mock<ITendersApiClient>().Object,
            pagesStore: pagesStore.Object,
            cache: cache);

        var warmed = await sut.TryWarmUpFromDiskAsync(CancellationToken.None);

        warmed.Should().BeFalse();
    }

    [Test]
    public async Task TryWarmUpFromDiskAsync_ShouldPopulateCache_AndReturnTrue_WhenCachedPagesExist()
    {
        var page1 = SamplePage(page: 1, id: 10, amount: 20m);
        var page2 = SamplePage(page: 2, id: 11, amount: 30m);

        var pagesStore = new Mock<ITendersPagesStore>();
        pagesStore
            .Setup(s => s.TryReadAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(page1);
        pagesStore
            .Setup(s => s.TryReadAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(page2);
        pagesStore
            .Setup(s => s.TryReadAsync(It.Is<int>(p => p != 1 && p != 2), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TendersGuruPagedResponse<TenderListItemApiModel>?)null);

        var cache = new MemoryCache(new MemoryCacheOptions());

        var sut = CreateSut(
            options: new TendersOptions { BaseUrl = "https://example.test", MaxSourcePages = 3 },
            apiClient: new Mock<ITendersApiClient>().Object,
            pagesStore: pagesStore.Object,
            cache: cache);

        var warmed = await sut.TryWarmUpFromDiskAsync(CancellationToken.None);

        warmed.Should().BeTrue();

        var index = await sut.GetIndexAsync(CancellationToken.None);
        index.Select(t => t.Id).Should().BeEquivalentTo([10, 11]);
    }

    [Test]
    public async Task RefreshAsync_ShouldFetchFromApi_WritePagesToDisk_AndPopulateCache()
    {
        var apiClient = new Mock<ITendersApiClient>();
        apiClient
            .Setup(c => c.GetTendersPageAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SamplePageWithAwardedSuppliers(page: 1));

        var pagesStore = new Mock<ITendersPagesStore>();
        pagesStore
            .Setup(s => s.WriteAsync(1, It.IsAny<TendersGuruPagedResponse<TenderListItemApiModel>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cache = new MemoryCache(new MemoryCacheOptions());

        var sut = CreateSut(
            options: new TendersOptions { BaseUrl = "https://example.test", MaxSourcePages = 1, RefreshDataInHours = 1 },
            apiClient: apiClient.Object,
            pagesStore: pagesStore.Object,
            cache: cache);

        await sut.RefreshAsync(CancellationToken.None);

        var index = await sut.GetIndexAsync(CancellationToken.None);
        index.Should().HaveCount(1);

        // Mapping details: awarded_value_eur -> Amount, suppliers are de-duplicated by id and blank names are ignored.
        var tender = index[0];
        tender.Amount.Should().Be(99m);
        tender.Suppliers.Select(s => (s.Id, s.Name)).Should().BeEquivalentTo([
            (1, "Supplier A"),
            (2, "Supplier B")
        ]);

        pagesStore.Verify(s => s.WriteAsync(1, It.IsAny<TendersGuruPagedResponse<TenderListItemApiModel>>(), It.IsAny<CancellationToken>()), Times.Once);
        apiClient.Verify(c => c.GetTendersPageAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RefreshAsync_ShouldFallbackToDiskCache_WhenApiFails()
    {
        var apiClient = new Mock<ITendersApiClient>();
        apiClient
            .Setup(c => c.GetTendersPageAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var pagesStore = new Mock<ITendersPagesStore>();
        pagesStore
            .Setup(s => s.TryReadAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SamplePage(page: 1, id: 77, amount: 10m));

        var cache = new MemoryCache(new MemoryCacheOptions());

        var sut = CreateSut(
            options: new TendersOptions { BaseUrl = "https://example.test", MaxSourcePages = 1, RefreshDataInHours = 1 },
            apiClient: apiClient.Object,
            pagesStore: pagesStore.Object,
            cache: cache);

        await sut.RefreshAsync(CancellationToken.None);

        var index = await sut.GetIndexAsync(CancellationToken.None);
        index.Select(t => t.Id).Should().ContainSingle().Which.Should().Be(77);

        pagesStore.Verify(s => s.TryReadAsync(1, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    private static TendersIndexProvider CreateSut(
        TendersOptions options,
        ITendersApiClient apiClient,
        ITendersPagesStore pagesStore,
        IMemoryCache cache)
    {
        return new TendersIndexProvider(
            apiClient,
            pagesStore,
            cache,
            Options.Create(options),
            NullLogger<TendersIndexProvider>.Instance);
    }

    private static TendersGuruPagedResponse<TenderListItemApiModel> SamplePage(int page, int id, decimal amount)
    {
        return new TendersGuruPagedResponse<TenderListItemApiModel>(
            PageCount: 10,
            PageNumber: page,
            PageSize: 1,
            Total: 1,
            Data:
            [
                new TenderListItemApiModel(
                    Id: id,
                    Date: new DateOnly(2025, 01, 01),
                    Title: "T",
                    Description: "D",
                    AwardedValueEur: amount,
                    Awarded: null)
            ]);
    }

    private static TendersGuruPagedResponse<TenderListItemApiModel> SamplePageWithAwardedSuppliers(int page)
    {
        return new TendersGuruPagedResponse<TenderListItemApiModel>(
            PageCount: 10,
            PageNumber: page,
            PageSize: 1,
            Total: 1,
            Data:
            [
                new TenderListItemApiModel(
                    Id: 123,
                    Date: new DateOnly(2025, 01, 02),
                    Title: "T",
                    Description: "D",
                    AwardedValueEur: 99m,
                    Awarded:
                    [
                        new AwardedSuppliersApiModel(
                            Suppliers:
                            [
                                new SupplierApiModel(1, "Supplier A"),
                                new SupplierApiModel(1, "Supplier A (duplicate)"),
                                new SupplierApiModel(2, "Supplier B"),
                                new SupplierApiModel(3, "   ")
                            ])
                    ])
            ]);
    }
}
