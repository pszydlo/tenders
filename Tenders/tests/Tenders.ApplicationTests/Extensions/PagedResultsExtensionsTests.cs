using FluentAssertions;
using Tenders.Application.Extensions;
using Tenders.Domain.Models;

namespace Tenders.ApplicationTests.Extensions;

[TestFixture]
public class PagedResultsExtensionsTests
{
    [Test]
    public void ToDto_ShouldMapItemsAndCopyPagingMetadata()
    {
        var paged = new PagedResult<int>(
            Items: [1, 2, 3],
            PageNumber: 2,
            PageSize: 3,
            TotalItems: 10,
            TotalPages: 4);

        var dto = paged.ToDto(i => $"v{i}");

        dto.Items.Should().Equal(["v1", "v2", "v3"]);
        dto.PageNumber.Should().Be(2);
        dto.PageSize.Should().Be(3);
        dto.TotalItems.Should().Be(10);
        dto.TotalPages.Should().Be(4);
    }
}
