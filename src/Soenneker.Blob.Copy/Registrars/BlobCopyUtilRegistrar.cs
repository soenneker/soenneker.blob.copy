using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blob.Copy.Abstract;

namespace Soenneker.Blob.Copy.Registrars;

/// <summary>
/// A utility library for Azure Blob storage copy operations
/// </summary>
public static class BlobCopyUtilRegistrar
{
    /// <summary>
    /// Adds blob copy as singleton.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The result of the operation.</returns>
    public static IServiceCollection AddBlobCopyAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IBlobCopyUtil, BlobCopyUtil>();

        return services;
    }

    /// <summary>
    /// Adds blob copy as scoped.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The result of the operation.</returns>
    public static IServiceCollection AddBlobCopyAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IBlobCopyUtil, BlobCopyUtil>();

        return services;
    }
}