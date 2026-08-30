using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Soenneker.Blob.Copy.Abstract;

/// <summary>
/// Performs Azure server-side blob copies and waits for their terminal status.
/// </summary>
public interface IBlobCopyUtil
{
    /// <summary>
    /// Copies a source blob to a destination, skipping missing sources, identical content, and self-copies.
    /// </summary>
    /// <param name="source">Source blob. It must carry a usable SAS URI or support generating one.</param>
    /// <param name="target">Destination blob to create or replace.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The completed copy operation, or <c>null</c> when no copy was needed.</returns>
    ValueTask<CopyFromUriOperation?> ServerSideBlobCopy(BlobClient source, BlobClient target, CancellationToken cancellationToken = default);
}
