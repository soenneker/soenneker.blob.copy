[![](https://img.shields.io/nuget/v/Soenneker.Blob.Copy.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blob.Copy/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blob.copy/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blob.copy/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Blob.Copy.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blob.Copy/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blob.copy/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blob.copy/actions/workflows/codeql.yml)

# Soenneker.Blob.Copy

Starts an Azure server-side blob copy, waits for completion, and skips missing sources, self-copies, and destinations with the same content hash.

## Install

```bash
dotnet add package Soenneker.Blob.Copy
```

Register the stateless utility in `Program.cs`:

```csharp
using Soenneker.Blob.Copy.Registrars;

builder.Services.AddBlobCopyAsSingleton();
```

Scoped registration is also available.

## Copy a blob

```csharp
using Azure.Storage.Blobs.Models;
using Soenneker.Blob.Copy.Abstract;

CopyFromUriOperation? operation = await blobCopy.ServerSideBlobCopy(
    sourceBlob,
    destinationBlob,
    cancellationToken);

if (operation is null)
{
    // The source was missing, the URIs targeted the same blob,
    // or both blobs exposed the same non-empty content hash.
}
```

The destination container is created privately if it does not exist. An existing destination blob is replaced by the Azure copy operation; it is not deleted first, so a failure to start the copy does not create an avoidable delete-before-copy gap.

## Source authorization

Azure Storage performs the transfer, so the service must be able to read the source URI:

- If `source.CanGenerateSasUri` is true, the utility generates a read-only SAS valid for one hour.
- Otherwise, the source URI is passed as-is. It must already contain usable authorization or identify a publicly readable blob.

The source credential must also allow reading properties because the utility checks existence and may compare content hashes. The destination client must allow container creation, property reads, and blob writes.

Full source and destination query strings are never written to this utility's logs, preventing SAS tokens from being exposed there.

## Result and failure behavior

- The returned `CopyFromUriOperation` has reached a terminal status when the method returns.
- A missing source or skipped copy returns `null`; it is not treated as an exception.
- Matching content is skipped only when both blobs expose non-empty content hashes. If hashes are absent or properties cannot be compared, the copy proceeds.
- The utility polls for up to five minutes. A timeout stops waiting but does not guarantee Azure stopped the server-side copy.
- A non-success Azure copy status throws `InvalidOperationException`; Azure request failures propagate as `RequestFailedException`.

- Cancellation stops pending work; it does not undo work that has already completed.
- Cancellation after Azure accepts the copy may leave a server-side operation running. Inspect or abort the destination copy explicitly when that distinction matters.
