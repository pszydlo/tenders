using FluentAssertions;
using Tenders.Application.Common;
using Tenders.Domain.Models;

namespace Tenders.ApplicationTests.Common;

[TestFixture]
public class ParseEnumTests
{
    [Test]
    public void ParseEnumOrDefault_ShouldReturnDefault_WhenValueIsNullOrWhitespace()
    {
        ParseEnum.ParseEnumOrDefault<OrderDirection>(null, OrderDirection.Desc).Should().Be(OrderDirection.Desc);
        ParseEnum.ParseEnumOrDefault<OrderDirection>("", OrderDirection.Desc).Should().Be(OrderDirection.Desc);
        ParseEnum.ParseEnumOrDefault<OrderDirection>("   ", OrderDirection.Desc).Should().Be(OrderDirection.Desc);
    }

    [Test]
    public void ParseEnumOrDefault_ShouldParseNumericValue_WhenDefined()
    {
        // OrderDirection.Asc = 0, Desc = 1
        ParseEnum.ParseEnumOrDefault<OrderDirection>("0", OrderDirection.Desc).Should().Be(OrderDirection.Asc);
        ParseEnum.ParseEnumOrDefault<OrderDirection>("1", OrderDirection.Asc).Should().Be(OrderDirection.Desc);
    }

    [Test]
    public void ParseEnumOrDefault_ShouldParseName_IgnoringCase()
    {
        ParseEnum.ParseEnumOrDefault<OrderDirection>("asc", OrderDirection.Desc).Should().Be(OrderDirection.Asc);
        ParseEnum.ParseEnumOrDefault<OrderDirection>("DeSc", OrderDirection.Asc).Should().Be(OrderDirection.Desc);
    }

    [Test]
    public void ParseEnumOrDefault_ShouldReturnDefault_WhenValueIsUnknown()
    {
        ParseEnum.ParseEnumOrDefault<OrderDirection>("not-a-real-value", OrderDirection.Desc).Should().Be(OrderDirection.Desc);
    }
}
