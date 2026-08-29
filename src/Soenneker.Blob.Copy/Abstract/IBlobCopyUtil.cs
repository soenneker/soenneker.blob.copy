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
    /// Returns the value produced by server Side Blob Copy.
    /// </summary>
    /// <param name="source">source to read or transform.</param>
    /// <param name="target">Target for the server side blob copy operation.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested copy From URI Operation.</returns>
    ValueTask<CopyFromUriOperation?> ServerSideBlobCopy(BlobClient source, BlobClient target, CancellationToken cancellationToken = default);
}
