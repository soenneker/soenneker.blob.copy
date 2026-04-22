using AwesomeAssertions;
using Soenneker.Blob.Copy.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Blob.Copy.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class BlobCopyUtilTests : HostedUnitTest
{
    private readonly IBlobCopyUtil _util;

    public BlobCopyUtilTests(Host host) : base(host)
    {
        _util = Resolve<IBlobCopyUtil>(true);
    }

    [Test]
    public void Resolve_should_resolve()
    {
        _util.Should().NotBeNull();
    }
}