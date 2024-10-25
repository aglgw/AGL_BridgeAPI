using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace AGL.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
                services.AddDbContext<OAPI_DbContext>(options =>
                {
                    options.UseLazyLoadingProxies().UseSqlServer(configuration["OAPI.Application.ConnectionString"],
                              sqlServerOptionsAction: sqlOptions =>
                              {
                                  sqlOptions.MigrationsAssembly(typeof(OAPI_DbContext).GetTypeInfo().Assembly.FullName);
                                  sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                              });
                }, ServiceLifetime.Scoped);

                services.AddScoped<OAPI_DbContext>();

                return services;
        }
    }
}
