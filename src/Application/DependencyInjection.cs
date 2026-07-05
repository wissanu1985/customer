using Application.Commons.Behaviours;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Mediator (source-generated) — scoped to match DbContext lifetime
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors =
            [
                typeof(UnhandledExceptionBehaviour<,>),
                typeof(ValidationBehaviour<,>),
                typeof(PerformanceBehaviour<,>)
            ];
        });

        // FluentValidation — auto-register all AbstractValidator<> in Application assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
