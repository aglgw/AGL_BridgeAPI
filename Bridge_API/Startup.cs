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
            // 인증 미들웨어1
            services.AddDbContext<OAPI_DbContext_GetSupplier>(options =>
                options.UseSqlServer(Configuration["OAPI.Application.ConnectionString"]));

            services.AddScoped<IOAPIDbContext, OAPI_DbContext_GetSupplier>(); // 인증 미들웨어 dbcontext
            services.AddScoped<IMyDatabaseService, MyDatabaseService>(); // 인증 미들웨어 dbcontext

            services.AddTransient<TeeTimeService>();
            services.AddSingleton<RequestQueue>(); // 티타임 수정 queue 처리
            services.AddHostedService<BackgroundRequestService>(); // 티타임 수정 queue 처리

            services.AddHostedService<MemoryMonitoringService>(); // 메모리 처리

            services.AddInfrastructure(Configuration);
            services.AddCustomIntegrations();
            services.AddCustomMvc(Logger);

            services.AddApiClient();
            services.AddDefaultCors();

            services.AddSingleton<IRedisService, RedisService>();

            // ModelState 유효성 검사 필터를 비활성화
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
                // C# Object 의 Summary 주석 읽어서 Swagger UI에 나오도록 설정
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Authorization 헤더 정의 추가
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // Authorization 요구사항 설정
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
                    var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>().EnvironmentName; // 현재 환경 이름 가져오기

                    if (actionDescriptor != null)
                    {
                        // 메서드에 EnvironmentSpecificAttribute가 있는지 확인
                        var environmentSpecificAttribute = actionDescriptor.MethodInfo
                            .GetCustomAttributes(typeof(EnvironmentSpecificAttribute), false)
                            .FirstOrDefault() as EnvironmentSpecificAttribute;

                        if (environmentSpecificAttribute != null)
                        {
                            // 현재 환경이 허용된 환경 중 하나인지 확인
                            return environmentSpecificAttribute.IsEnvironmentAllowed(environment);
                        }

                        // 속성이 없는 메서드는 모든 환경에서 보이도록 설정
                        return true;
                    }

                    return false; // 기본적으로 숨김
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
                    c.DefaultModelsExpandDepth(-1); // 스키마 제거
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
