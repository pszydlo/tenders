using FluentAssertions;
using Tenders.Application.Extensions;
using Tenders.Domain.Models;

namespace Tenders.ApplicationTests.Extensions;

[TestFixture]
public class MappingExtensionsTests
{
    [Test]
    public void Supplier_ToDto_ShouldMapIdAndName()
    {
        var supplier = new Supplier(12, "ACME");

        var dto = supplier.ToDto();

        dto.Id.Should().Be(12);
        dto.Name.Should().Be("ACME");
    }

    [Test]
    public void Tender_ToDto_ShouldMapAllFields_AndMapSuppliers()
    {
        var tender = new Tender(
            Id: 101,
            Date: new DateOnly(2025, 12, 31),
            Title: "Title",
            Description: "Description",
            Amount: 123.45m,
            Suppliers: [
                new Supplier(1, "S1"),
                new Supplier(2, "S2")
            ]);

        var dto = TendersExtensions.ToDto(tender);

        dto.Id.Should().Be(101);
        dto.Date.Should().Be(new DateOnly(2025, 12, 31));
        dto.Title.Should().Be("Title");
        dto.Description.Should().Be("Description");
        dto.Amount.Should().Be(123.45m);
        dto.Suppliers.Should().HaveCount(2);
        dto.Suppliers.Select(s => (s.Id, s.Name)).Should().BeEquivalentTo([
            (1, "S1"),
            (2, "S2")
        ]);
    }
}
