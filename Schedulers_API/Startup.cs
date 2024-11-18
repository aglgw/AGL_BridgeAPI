using AGL.Api.ApplicationCore;
using AGL.Api.ApplicationCore.Filters;
using AGL.Api.ApplicationCore.Middleware;
using AGL.Api.ApplicationCore.Models;
using AGL.Api.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Quartz.Impl;
using Quartz.Logging;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.IO;
using System.Reflection;

namespace AGL.Api.Schedulers_API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public ILoggerFactory Logger { get; }
        public Startup(IConfiguration configuration, ILoggerFactory logger)
        {
            Configuration = configuration;
            Logger = logger;
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationCore();
            services.AddInfrastructure(Configuration);
            services.AddCustomIntegrations();
            services.AddCustomMvc(Logger);
            
            services.AddApiClient();
            services.AddDefaultCors();

            services.Configure<AppSettings>(Configuration);

            var runScheduler = Configuration.GetValue<string>("Scheduler");


            if (string.Equals(runScheduler, "on"))
            {
                LogProvider.SetCurrentLogProvider(new QuartzLogProvider());


                foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Namespace == "AGL.Api.Schedulers_API.Jobs"))
                {
                    services.AddScoped(type);
                }
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Namespace == "AGL.Api.Schedulers_API.Schedulers"))
                {
                    services.AddScoped(type);
                }
                services.AddSingleton<QuartzJobRunner>();
                services.AddSingleton<QuartzJobFactory>();
                services.AddScoped<QuartzSchedule.Startup>();
                services.AddScoped<QuartzSchedulerListener>();
                var scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();
                services.AddSingleton(scheduler);
                services.AddHostedService<QuartzHostedService>();
            }

            var openApi = Configuration.GetSection("OpenApi").Get<OpenApiConfiguration>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(openApi.Version, new OpenApiInfo
                {
                    Version = openApi.Version,
                    Title = openApi.Title,
                    Description = openApi.Description
                });
                // C# Object 의 Summary 주석 읽어서 Swagger UI에 나오도록 설정
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                

            });

            services.AddSwaggerGenNewtonsoftSupport(); // Swagger NewtonJson 확장 추가
            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly()); // Swagger UI Example Value C# Object로 추가
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var openApi = Configuration.GetSection("OpenApi").Get<OpenApiConfiguration>();

            app.ExceptionHandler();
            app.UseRequestLoggerMiddleware();

            if (env.EnvironmentName == "SandBox" || env.EnvironmentName == "Development")
            {
                // 스웨거는 테스트를 위한 API만 제공. 리얼은 스웨거 제공하지 않음
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{openApi.Title} {openApi.Version}");
                    // https://stackoverflow.com/questions/60159998/how-to-configure-swashbuckle-to-omit-template-entity-schema-from-the-documen
                    c.DefaultModelsExpandDepth(-1); // 스키마 제거
                });
            }
            
            app.UseHttpsRedirection();

            app.UseCors("default");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
