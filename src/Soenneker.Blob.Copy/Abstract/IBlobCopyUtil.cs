using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Soenneker.Blob.Copy.Abstract;

/// <summary>
/// A utility library for Azure Blob storage copy operations <para/>
/// </summary>
public interface IBlobCopyUtil
{
    /// <summary>
    /// Executes the server side blob copy operation.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    ValueTask<CopyFromUriOperation?> ServerSideBlobCopy(BlobClient source, BlobClient target, CancellationToken cancellationToken = default);
}