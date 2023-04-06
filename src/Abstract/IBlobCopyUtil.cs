using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Soenneker.Blob.Copy.Abstract;

/// <summary>
/// A utility library for Azure Blob storage copy operations <para/>
/// </summary>
public interface IBlobCopyUtil
{
    ValueTask<CopyFromUriOperation?> ServerSideBlobCopy(BlobClient source, BlobClient target);
}