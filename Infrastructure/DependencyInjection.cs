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
            services.AddDbContext<CmsDbContext>(options =>
            {
                options.UseLazyLoadingProxies().UseSqlServer(configuration["CMS.Application.ConnectionString"],
                          sqlServerOptionsAction: sqlOptions =>
                          {
                              sqlOptions.MigrationsAssembly(typeof(CmsDbContext).GetTypeInfo().Assembly.FullName);
                              sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                          });
            }, ServiceLifetime.Scoped);

            services.AddDbContext<HttDbContext>(options =>
            {
                options.UseLazyLoadingProxies().UseSqlServer(configuration["HTT.Application.ConnectionString"],
                          sqlServerOptionsAction: sqlOptions =>
                          {
                              sqlOptions.MigrationsAssembly(typeof(HttDbContext).GetTypeInfo().Assembly.FullName);
                              sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                          });
            }, ServiceLifetime.Scoped);



            //services.AddDbContext<AdminContext>(options => options.UseMySql(configuration["ADM.Application.ConnectionString"],
            //    ServerVersion.AutoDetect(configuration["ADM.Application.ConnectionString"]),
            //    mySqlOptionsAction: sqlOptions =>
            //    {
            //        //DB 연결에 실패할 경우 재시도 설정            
            //        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            //    }));


                return services;
        }
    }
}
