using AGL.Api.Domain.Entities.OAPI;
using AGL.Api.Domain.Entities.OTA;
using AGL.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AGL.API.Infrastructure.Data
{
    public class OTADbContext : DbContext
    {
        private readonly ILogger<OTADbContext> _logger;

        public OTADbContext(
            DbContextOptions<OTADbContext> options,
            ILogger<OTADbContext> logger = null) : base(options)
        {
            _logger = logger;
        }

        public DbSet<OTA_Authentication> Authentications { get; set; } // 인증
        public DbSet<OTA_SyncClient> SyncClients { get; set; } // 싱크 클라이언트 (내부 혹은 외부 채널)

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // OAPI_Authentication 테이블을 두 엔터티와 공유
            builder.Entity<OTA_Authentication>().ToTable("OAPI_Authentication"); // 실제 테이블 이름
            builder.Entity<OAPI_Authentication>().ToTable("OAPI_Authentication"); // 동일한 테이블
            // OAPI_SyncClient 테이블을 두 엔터티와 공유
            builder.Entity<OTA_SyncClient>().ToTable("OAPI_SyncClient"); // 실제 테이블 이름
            builder.Entity<OAPI_SyncClient>().ToTable("OAPI_SyncClient"); // 동일한 테이블

            foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
