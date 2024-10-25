using AGL.Api.ApplicationCore;
using AGL.Api.ApplicationCore.Filters;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Middleware;
using AGL.Api.ApplicationCore.Models;
using AGL.Api.Infrastructure;
using AGL.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using System;
using System.IO;
using System.Reflection;

namespace AGL.Api.API_Template
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var openApi = Configuration.GetSection("OpenApi").Get<OpenApiConfiguration>();

            app.ExceptionHandler();
            app.UseRequestLoggerMiddleware();

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{openApi.Title} {openApi.Version}");
                // https://stackoverflow.com/questions/60159998/how-to-configure-swashbuckle-to-omit-template-entity-schema-from-the-documen
                c.DefaultModelsExpandDepth(-1); // 스키마 제거
            });

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
