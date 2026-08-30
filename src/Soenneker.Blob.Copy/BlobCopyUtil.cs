using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Soenneker.Blob.Copy.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.Delay;

namespace Soenneker.Blob.Copy;

/// <inheritdoc cref="IBlobCopyUtil"/>
public sealed class BlobCopyUtil : IBlobCopyUtil
{
    private readonly ILogger<BlobCopyUtil> _logger;

    public BlobCopyUtil(ILogger<BlobCopyUtil> logger)
    {
        _logger = logger;
    }

    public async ValueTask<CopyFromUriOperation?> ServerSideBlobCopy(BlobClient source, BlobClient target, CancellationToken cancellationToken = default)
    {
        if (!await source.ExistsAsync(cancellationToken)
                         .NoSync())
        {
            _logger.LogError("Attempted to copy a blob that does not exist: {source}", GetSafeUri(source.Uri));
            return null;
        }

        _logger.LogInformation("File transfer started: {source} to {target}", GetSafeUri(source.Uri), GetSafeUri(target.Uri));

        if (!await target.GetParentBlobContainerClient()
                         .ExistsAsync(cancellationToken)
                         .NoSync())
        {
            await target.GetParentBlobContainerClient()
                        .CreateIfNotExistsAsync(cancellationToken: cancellationToken)
                        .NoSync();
        }

        // Delete target if exists
        if (await target.ExistsAsync(cancellationToken)
                        .NoSync())
        {
            if (!await ShouldCopy(source, target, cancellationToken)
                    .NoSync())
            {
                _logger.LogInformation("Skipping copy from {source} to {target}.", source.Uri.AbsolutePath, target.Uri.AbsolutePath);
                return null;
            }

        }

        Uri sourceUri = source.CanGenerateSasUri
            ? source.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1))
            : source.Uri;

        CopyFromUriOperation result = await target.StartCopyFromUriAsync(sourceUri, cancellationToken: cancellationToken)
                                                  .NoSync();

        await GetBlobCopyStatus(target, cancellationToken)
            .NoSync();

        _logger.LogInformation("File transfer from {source} to {target} completed", GetSafeUri(source.Uri), GetSafeUri(target.Uri));

        return result;
    }

    private async ValueTask GetBlobCopyStatus(BlobBaseClient blobClient, CancellationToken cancellationToken)
    {
        CopyStatus status = (await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken)
                                             .NoSync()).Value.CopyStatus;

        if (status == CopyStatus.Pending)
        {
            DateTimeOffset started = DateTimeOffset.UtcNow;

            TimeSpan fiveMin = TimeSpan.FromMinutes(5);

            while (status == CopyStatus.Pending)
            {
                if (DateTimeOffset.UtcNow.Subtract(started) > fiveMin)
                {
                    throw new TimeoutException($"Timed out while waiting for the blob copy at {GetSafeUri(blobClient.Uri)}.");
                }

                await DelayUtil.Delay(1000, _logger, cancellationToken)
                               .NoSync();

                status = (await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken)
                                          .NoSync()).Value.CopyStatus;

                _logger.LogDebug("Waiting on copy {uri} to finish...", GetSafeUri(blobClient.Uri));
            }
        }

        if (status != CopyStatus.Success)
        {
            throw new InvalidOperationException($"Blob copy at {GetSafeUri(blobClient.Uri)} finished with status {status}.");
        }
    }

    /// <summary>
    /// Determines whether a server-side copy would change the destination.
    /// </summary>
    private async ValueTask<bool> ShouldCopy(BlobBaseClient blobA, BlobBaseClient blobB, CancellationToken cancellationToken)
    {
        if (string.Equals(blobA.Uri.Host, blobB.Uri.Host, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(blobA.Uri.AbsolutePath, blobB.Uri.AbsolutePath, StringComparison.Ordinal))
        {
            _logger.LogWarning("Skipping a copy whose source and destination are the same blob: {uri}", GetSafeUri(blobA.Uri));
            return false;
        }

        try
        {
            Response<BlobProperties> blobAProperties = await blobA.GetPropertiesAsync(cancellationToken: cancellationToken)
                                                                 .NoSync();
            Response<BlobProperties> blobBProperties = await blobB.GetPropertiesAsync(cancellationToken: cancellationToken)
                                                                 .NoSync();
            byte[]? sourceHash = blobAProperties.Value.ContentHash;
            byte[]? targetHash = blobBProperties.Value.ContentHash;

            if (sourceHash is { Length: > 0 } && targetHash is { Length: > 0 } && sourceHash.SequenceEqual(targetHash))
            {
                _logger.LogInformation("Skipping copy because source and destination content hashes match");
                return false;
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            _logger.LogWarning(e, "Could not compare source and destination properties; the copy will proceed");
        }

        return true;
    }

    private static string GetSafeUri(Uri uri) => uri.GetLeftPart(UriPartial.Path);
}
