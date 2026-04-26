using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register application services
            
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg=>
                {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                }
            );

            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
