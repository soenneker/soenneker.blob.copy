using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Soenneker.Blob.Copy.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Blob.Copy;

///<inheritdoc cref="IBlobCopyUtil"/>
public class BlobCopyUtil : IBlobCopyUtil
{
    private readonly ILogger<BlobCopyUtil> _logger;
        
    public BlobCopyUtil(ILogger<BlobCopyUtil> logger)
    {
        _logger = logger;
    }

    public async ValueTask<CopyFromUriOperation?> ServerSideBlobCopy(BlobClient source, BlobClient target, CancellationToken cancellationToken = default)
    {
        if (!await source.ExistsAsync(cancellationToken).NoSync())
        {
            _logger.LogError("*** Attempted to copy a blob that doesn't exist: {source} ***", source.Uri.AbsoluteUri);
            return null;
        }

        _logger.LogInformation("File transfer started: {source} to {target}", source.Uri, target.Uri);

        if (!await target.GetParentBlobContainerClient().ExistsAsync(cancellationToken).NoSync())
        {
            _logger.LogInformation("Creating container {container} because it doesn't exist", target.BlobContainerName);
            await target.GetParentBlobContainerClient().CreateIfNotExistsAsync(cancellationToken: cancellationToken).NoSync();
        }

        // Delete target if exists
        if (await target.ExistsAsync(cancellationToken).NoSync())
        {
            if (!await ShouldCopy(source, target, cancellationToken).NoSync())
            {
                _logger.LogInformation("Skipping copy from {source} to {target}.", source.Uri.AbsolutePath, target.Uri.AbsolutePath);
                return null;
            }

            _logger.LogInformation("Deleting non-identical existing blob at target: {target}", target.Uri);

            await target.DeleteAsync(cancellationToken: cancellationToken).NoSync();
        }

        Uri? sasUri = source.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(5));

        CopyFromUriOperation result = await target.StartCopyFromUriAsync(sasUri, cancellationToken: cancellationToken).NoSync();

        await GetBlobCopyStatus(target, cancellationToken).NoSync();

        _logger.LogInformation("Success: File transfer operation for {source} to {target} completed!", source.Uri, target.Uri);

        return result;
    }

    private async ValueTask GetBlobCopyStatus(BlobBaseClient blobClient, CancellationToken cancellationToken)
    {
        CopyStatus status = (await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken).NoSync()).Value.CopyStatus;

        if (status == CopyStatus.Pending)
        {
            DateTime started = DateTime.UtcNow;

            TimeSpan fiveMin = TimeSpan.FromMinutes(5);

            while (status == CopyStatus.Pending)
            {
                if (DateTime.UtcNow.Subtract(started) > fiveMin)
                {
                    throw new Exception($"Copy timed out {blobClient.Uri}, aborting wait");
                }

                await Task.Delay(1000, cancellationToken).NoSync();

                status = (await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken).NoSync()).Value.CopyStatus;

                _logger.LogDebug("Waiting on copy {uri} to finish...", blobClient.Uri);
            }
        }

        if (status != CopyStatus.Success)
        {
            throw new Exception($"Failed downloading blob at {blobClient.Uri}. Blob copy operation status {status}");
        }
    }

    /// <summary>
    /// Determines if a server side copy should be performed based on criteria:
    /// 1. If blob paths are EXACTLY the same, log error, do not copy
    /// 2. If blob content is identical AND blobs are going to the same location (relative to storage account)
    /// </summary>
    private async ValueTask<bool> ShouldCopy(BlobBaseClient blobA, BlobBaseClient blobB, CancellationToken cancellationToken)
    {
        // Check entire url
        if (blobA.Uri.AbsoluteUri == blobB.Uri.AbsoluteUri)
        {
            _logger.LogError("WARNING Attempted to copy blob to same exact destination: {uri}", blobA.Uri.AbsoluteUri);
            return false;
        }

        // AbsolutePath is the part of the url after the storage account (i.e. "/container/blob")
        if (blobA.Uri.AbsolutePath == blobB.Uri.AbsolutePath)
        {
            try
            {
                Response<BlobProperties>? blobAPropsTask = await blobA.GetPropertiesAsync(cancellationToken: cancellationToken).NoSync();
                Response<BlobProperties>? blobBPropsTask = await blobB.GetPropertiesAsync(cancellationToken: cancellationToken).NoSync();
                // TODO: pretty sure we can use task.whenall but being careful for now

                return blobAPropsTask.Value.ContentHash == blobBPropsTask.Value.ContentHash;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting properties from blobs");
            }
        }

        return true;
    }
}