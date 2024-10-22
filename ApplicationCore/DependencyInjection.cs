using AGL.Api.ApplicationCore.Filters;
using AGL.Api.ApplicationCore.Handler;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Infrastructure.ApiClient;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace AGL.Api.ApplicationCore
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationCore(this IServiceCollection services)
        {
            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            return services;
        }

        public static IServiceCollection AddApiClient(this IServiceCollection services)
        {
            
            services.AddTransient<HttpClientRequestIdDelegatingHandler>();
            services.AddTransient<IIdentifiable, Identifiable>();
            services.AddSingleton<IIdentityService, IdentityService>();

            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddHttpMessageHandler<HttpClientRequestIdDelegatingHandler>();

            return services;
        }
    }

    public static class CustomExtensionsMethods
    {
        public static IServiceCollection AddCustomIntegrations(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            services.Scan(scan => scan
                .FromCallingAssembly()
                .FromApplicationDependencies(a => a.FullName.StartsWith("AGL.Api"))
                .AddClasses(publicOnly: true)
                .AsMatchingInterface((service, filter) =>
                    filter.Where(implementation => implementation.Name.Equals($"I{service.Name}", StringComparison.OrdinalIgnoreCase)))
                .WithScopedLifetime()
            );

            return services;
        }

        public static IServiceCollection AddCustomMvc(this IServiceCollection services, ILoggerFactory logger)
        {
            services.AddOptions();
            // Add framework services.
            services.AddControllers(options =>
            {
                options.Filters.Add(new HttpResponseExceptionFilter(logger));
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                //options.SerializerSettings.Converters.Add(new StringEnumConverter()); // Enum 문자열로 사용 가능
                options.UseCamelCasing(true); // camelCase 서식 사용
            }); ;

            services.Configure<WebEncoderOptions>(options => {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All); // 한글이 인코딩되는 문제 해결
            });

            return services;
        }

        

        public static IServiceCollection AddDefaultCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("https://localhost:44361",
                                       "https://localhost:44390",
                                       "https://localhost:44390",
                                       "https://local-renew.heyteetime.com/"
                                       )
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            return services;
        }

        public static IServiceCollection AddApiCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("api", policy =>
                {
                    policy.WithOrigins("https://localhost:44361", "https://localhost:44362", "https://localhost:44390", "https://local-renew.heyteetime.com/")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
