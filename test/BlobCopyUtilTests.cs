using FluentAssertions;
using Soenneker.Blob.Copy.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.Blob.Copy.Tests;

[Collection("Collection")]
public class BlobCopyUtilTests : FixturedUnitTest
{
    private readonly IBlobCopyUtil _util;

    public BlobCopyUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IBlobCopyUtil>(true);
    }

    [Fact]
    public void Resolve_should_resolve()
    {
        _util.Should().NotBeNull();
    }
}