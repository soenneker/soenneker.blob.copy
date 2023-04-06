using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blob.Copy.Abstract;

namespace Soenneker.Blob.Copy.Registrars;

/// <summary>
/// A utility library for Azure Blob storage copy operations
/// </summary>
public static class BlobCopyUtilRegistrar
{
    public static void AddBlobCopyAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IBlobCopyUtil, BlobCopyUtil>();
    }

    public static void AddBlobCopyAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IBlobCopyUtil, BlobCopyUtil>();
    }
}