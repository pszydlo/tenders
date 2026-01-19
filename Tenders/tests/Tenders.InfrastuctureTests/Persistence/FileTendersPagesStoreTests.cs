using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Tenders.Infrastucture.Options;
using Tenders.Infrastucture.Persistence;
using Tenders.Infrastucture.TendersApi.Models;

namespace Tenders.InfrastuctureTests.Persistence;

[TestFixture]
public class FileTendersPagesStoreTests
{
    [Test]
    public void TryReadAsync_ShouldThrow_WhenPageNumberIsLessThan1()
    {
        var sut = CreateSut(persistPages: true, contentRoot: CreateTempDir());

        Func<Task> act = () => sut.TryReadAsync(0, CancellationToken.None);

        act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task TryReadAsync_ShouldReturnNull_WhenPersistDisabled()
    {
        var sut = CreateSut(persistPages: false, contentRoot: CreateTempDir());

        var result = await sut.TryReadAsync(1, CancellationToken.None);

        result.Should().BeNull();
    }

    [Test]
    public async Task WriteAsync_ShouldBeNoOp_WhenPersistDisabled()
    {
        var root = CreateTempDir();
        var sut = CreateSut(persistPages: false, contentRoot: root);

        await sut.WriteAsync(1, SamplePage(page: 1), CancellationToken.None);

        Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories).Should().BeEmpty();
    }

    [Test]
    public async Task WriteAsync_ThenTryReadAsync_ShouldRoundtrip_WhenPersistEnabled()
    {
        var root = CreateTempDir();
        var sut = CreateSut(persistPages: true, contentRoot: root);

        var page = SamplePage(page: 7);

        await sut.WriteAsync(7, page, CancellationToken.None);
        var read = await sut.TryReadAsync(7, CancellationToken.None);

        read.Should().NotBeNull();
        read!.PageNumber.Should().Be(7);
        read.Data.Should().HaveCount(1);
        read.Data[0].Id.Should().Be(123);
    }

    private static FileTendersPagesStore CreateSut(bool persistPages, string contentRoot)
    {
        var options = Options.Create(new TendersOptions
        {
            BaseUrl = "https://example.test",
            PersistPagesToDisk = persistPages,
            PagesCacheDirectory = "cache"
        });

        var env = new Mock<IHostEnvironment>();
        env.SetupGet(e => e.ContentRootPath).Returns(contentRoot);

        return new FileTendersPagesStore(options, env.Object, NullLogger<FileTendersPagesStore>.Instance);
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "tenders-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static TendersGuruPagedResponse<TenderListItemApiModel> SamplePage(int page)
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
                    AwardedValueEur: 10m,
                    Awarded: null)
            ]);
    }
}
