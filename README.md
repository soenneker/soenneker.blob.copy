[![](https://img.shields.io/nuget/v/Soenneker.Blob.Copy.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blob.Copy/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blob.copy/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blob.copy/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Blob.Copy.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blob.Copy/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blob.copy/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blob.copy/actions/workflows/codeql.yml)

# Soenneker.Blob.Copy

A utility library for Azure Blob storage copy operations.

## Install

```bash
dotnet add package Soenneker.Blob.Copy
```

## Quick start

```csharp
using Soenneker.Blob.Copy.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddBlobCopyAsSingleton();
```

Registers Blob Copy with a singleton lifetime.

## What you get

- `IBlobCopyUtil` — A utility library for Azure Blob storage copy operations.
- `BlobCopyUtilRegistrar` — A utility library for Azure Blob storage copy operations.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IBlobCopyUtil.ServerSideBlobCopy(source, target, cancellationToken)` | Returns the value produced by server Side Blob Copy. | A task whose result is the requested copy From URI Operation. |
| `BlobCopyUtilRegistrar.AddBlobCopyAsSingleton(services)` | Registers Blob Copy with a singleton lifetime. | The same service collection, so additional registrations can be chained. |
| `BlobCopyUtilRegistrar.AddBlobCopyAsScoped(services)` | Registers Blob Copy with a scoped lifetime. | The same service collection, so additional registrations can be chained. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
