using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Services;
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
using AGL.Api.ApplicationCore.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Hosting;


namespace AGL.Api.Bridge_API
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
            // ���� �̵����1
            services.AddDbContext<OAPI_DbContext_GetSupplier>(options =>
                options.UseSqlServer(Configuration["OAPI.Application.ConnectionString"]));

            services.AddScoped<IOAPIDbContext, OAPI_DbContext_GetSupplier>(); // ���� �̵���� dbcontext
            services.AddScoped<IMyDatabaseService, MyDatabaseService>(); // ���� �̵���� dbcontext

            services.AddTransient<TeeTimeService>();
            services.AddSingleton<RequestQueue>(); // ƼŸ�� ���� queue ó��
            services.AddHostedService<BackgroundRequestService>(); // ƼŸ�� ���� queue ó��

            services.AddHostedService<MemoryMonitoringService>(); // �޸� ó��

            services.AddInfrastructure(Configuration);
            services.AddCustomIntegrations();
            services.AddCustomMvc(Logger);

            services.AddApiClient();
            services.AddDefaultCors();

            services.AddSingleton<IRedisService, RedisService>();

            // ModelState ��ȿ�� �˻� ���͸� ��Ȱ��ȭ
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

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
                // C# Object �� Summary �ּ� �о Swagger UI�� �������� ����
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Authorization ��� ���� �߰�
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // Authorization �䱸���� ����
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionDescriptor = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    var serviceProvider = services.BuildServiceProvider();
                    var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>().EnvironmentName; // ���� ȯ�� �̸� ��������

                    if (actionDescriptor != null)
                    {
                        // �޼��忡 EnvironmentSpecificAttribute�� �ִ��� Ȯ��
                        var environmentSpecificAttribute = actionDescriptor.MethodInfo
                            .GetCustomAttributes(typeof(EnvironmentSpecificAttribute), false)
                            .FirstOrDefault() as EnvironmentSpecificAttribute;

                        if (environmentSpecificAttribute != null)
                        {
                            // ���� ȯ���� ���� ȯ�� �� �ϳ����� Ȯ��
                            return environmentSpecificAttribute.IsEnvironmentAllowed(environment);
                        }

                        // �Ӽ��� ���� �޼���� ��� ȯ�濡�� ���̵��� ����
                        return true;
                    }

                    return false; // �⺻������ ����
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var openApi = Configuration.GetSection("OpenApi").Get<OpenApiConfiguration>();

            app.ExceptionHandler();
            app.UseRequestLoggerMiddleware();

            if (env.EnvironmentName == "SandBox" || env.EnvironmentName == "Development")
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{openApi.Title} {openApi.Version}");
                    // https://stackoverflow.com/questions/60159998/how-to-configure-swashbuckle-to-omit-template-entity-schema-from-the-documen
                    c.DefaultModelsExpandDepth(-1); // ��Ű�� ����
                });
            }

            app.UseHttpsRedirection();

            app.UseCors("default");

            app.UseRouting();
            app.UseMiddleware<AuthenticationMiddleware>();

            //app.UseAuthenticationMiddleware();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
