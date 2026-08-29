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
    /// Registers Blob Copy with a singleton lifetime.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddBlobCopyAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IBlobCopyUtil, BlobCopyUtil>();

        return services;
    }

    /// <summary>
    /// Registers Blob Copy with a scoped lifetime.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddBlobCopyAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IBlobCopyUtil, BlobCopyUtil>();

        return services;
    }
}
