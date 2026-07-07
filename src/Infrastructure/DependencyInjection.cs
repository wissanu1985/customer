using Application.Commons.Services;
using Domain.Common;
using Domain.Repositories;
using Infrastructure.contexts;
using Infrastructure.Interceptors;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.IdCardImageStore;
using Infrastructure.Services.Typhoon;
using Microsoft.IO;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Default user provider — override in WebUi with auth-backed implementation.
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        // Audit interceptor is scoped because it depends on ICurrentUserProvider (scoped).
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<CustomerContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddDbContextFactory<CustomerContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
            // DbContextFactory-created contexts do not resolve scoped interceptors from DI;
            // they use a singleton interceptor instance. Audit for factory contexts is skipped
            // (factory is used for read-only/seed scenarios). Use the scoped DbContext for audited writes.
        }, ServiceLifetime.Scoped);

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<CustomerContext>());

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReadOnlyUnitOfWork, ReadOnlyUnitOfWork>();

        // RecyclableMemoryStream pool — singleton, shared across all requests
        services.AddSingleton<RecyclableMemoryStreamManager>();

        // Typhoon OCR — typed HttpClient with timeout from config
        services.AddHttpClient<ITyphoonOcrService, TyphoonOcrService>((sp, client) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var timeout = cfg.GetValue("Typhoon:OcrTimeoutSeconds", 60);
            client.Timeout = TimeSpan.FromSeconds(timeout);
        });

        // Typhoon Chat — OpenAI-compatible client
        services.AddScoped<ITyphoonChatService, TyphoonChatService>();

        // Encrypted ID card image store
        services.AddScoped<IIdCardImageStore, IdCardImageStore>();

        return services;
    }
}
