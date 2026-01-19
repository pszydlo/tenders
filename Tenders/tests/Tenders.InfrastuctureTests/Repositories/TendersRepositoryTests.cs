using FluentAssertions;
using Moq;
using Tenders.Domain.Models;
using Tenders.Infrastucture.Caching;
using Tenders.Infrastucture.Repositories;

namespace Tenders.InfrastuctureTests.Repositories;

[TestFixture]
public class TendersRepositoryTests
{
    [Test]
    public async Task GetByIdAsync_ShouldReturnMatchingTender_WhenExists()
    {
        var index = BuildIndex();
        var indexProvider = new Mock<ITendersIndexProvider>();
        indexProvider
            .Setup(p => p.GetIndexAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(index);

        var sut = new TendersRepository(indexProvider.Object);

        var result = await sut.GetByIdAsync(3, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(3);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTenderDoesNotExist()
    {
        var indexProvider = new Mock<ITendersIndexProvider>();
        indexProvider
            .Setup(p => p.GetIndexAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildIndex());

        var sut = new TendersRepository(indexProvider.Object);

        var result = await sut.GetByIdAsync(999, CancellationToken.None);

        result.Should().BeNull();
    }

    [Test]
    public async Task SearchAsync_ShouldFilterByPriceRange_AndSupplier_AndDateRange()
    {
        var indexProvider = new Mock<ITendersIndexProvider>();
        indexProvider
            .Setup(p => p.GetIndexAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildIndex());

        var sut = new TendersRepository(indexProvider.Object);

        var criteria = new TenderSearchCriteria(
            MinPriceEur: 50m,
            MaxPriceEur: 150m,
            DateFrom: new DateOnly(2024, 01, 01),
            DateTo: new DateOnly(2024, 12, 31),
            SupplierId: 2,
            PageSize: 50,
            PageNumber: 1,
            OrderBy: TenderOrderBy.Date,
            OrderDirection: OrderDirection.Asc);

        var result = await sut.SearchAsync(criteria, CancellationToken.None);

        // In BuildIndex(): only tender Id=2 matches (supplier 2, 100 EUR, 2024-06-10)
        result.TotalItems.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be(2);
    }

    [Test]
    public async Task SearchAsync_ShouldSortByPrice_Desc_ThenByIdDesc()
    {
        var indexProvider = new Mock<ITendersIndexProvider>();
        indexProvider
            .Setup(p => p.GetIndexAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildIndex());

        var sut = new TendersRepository(indexProvider.Object);

        var criteria = new TenderSearchCriteria(
            MinPriceEur: null,
            MaxPriceEur: null,
            DateFrom: null,
            DateTo: null,
            SupplierId: null,
            PageSize: 10,
            PageNumber: 1,
            OrderBy: TenderOrderBy.PriceEur,
            OrderDirection: OrderDirection.Desc);

        var result = await sut.SearchAsync(criteria, CancellationToken.None);

        result.Items.Select(t => t.Id).Should().Equal([4, 1, 2, 3]);
    }

    [Test]
    public async Task SearchAsync_ShouldReturnPagedData_WithCorrectTotals()
    {
        var indexProvider = new Mock<ITendersIndexProvider>();
        indexProvider
            .Setup(p => p.GetIndexAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildIndex());

        var sut = new TendersRepository(indexProvider.Object);

        // Default ordering: Date desc then Id desc.
        var criteria = new TenderSearchCriteria(
            MinPriceEur: null,
            MaxPriceEur: null,
            DateFrom: null,
            DateTo: null,
            SupplierId: null,
            PageSize: 2,
            PageNumber: 2,
            OrderBy: TenderOrderBy.Date,
            OrderDirection: OrderDirection.Desc);

        var result = await sut.SearchAsync(criteria, CancellationToken.None);

        result.TotalItems.Should().Be(4);
        result.TotalPages.Should().Be(2);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(2);

        // Page 1 items: [4,3], Page 2 items: [2,1]
        result.Items.Select(t => t.Id).Should().Equal([2, 1]);
    }

    private static IReadOnlyList<Tender> BuildIndex()
    {
        return new List<Tender>
        {
            new(
                Id: 1,
                Date: new DateOnly(2024, 01, 05),
                Title: "T1",
                Description: "",
                Amount: 200m,
                Suppliers: [new Supplier(1, "S1")] ),

            new(
                Id: 2,
                Date: new DateOnly(2024, 06, 10),
                Title: "T2",
                Description: "",
                Amount: 100m,
                Suppliers: [new Supplier(2, "S2"), new Supplier(3, "S3")] ),

            new(
                Id: 3,
                Date: new DateOnly(2025, 01, 01),
                Title: "T3",
                Description: "",
                Amount: 10m,
                Suppliers: [] ),

            new(
                Id: 4,
                Date: new DateOnly(2025, 05, 15),
                Title: "T4",
                Description: "",
                Amount: 300m,
                Suppliers: [new Supplier(2, "S2")] ),
        };
    }
}
