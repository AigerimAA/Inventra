using FluentValidation;
using Inventra.Application.Common.Behaviors;
using Inventra.Application.Common.Mappings;
using Inventra.Application.Inventories.Commands.CreateInventory;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Inventra.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateInventoryCommand).Assembly);

                cfg.AddBehavior(typeof(IPipelineBehavior<,>),
                    typeof(LoggingBehavior<,>));

                cfg.AddBehavior(typeof(IPipelineBehavior<,>),
                    typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(typeof(CreateInventoryCommand).Assembly);

            return services;
        }
    }
}
