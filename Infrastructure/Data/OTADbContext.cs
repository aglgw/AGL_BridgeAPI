using AGL.Api.Domain.Entities.OAPI;
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
        //private readonly ILogger<OTADbContext> _logger;

        //public OTADbContext(
        //    DbContextOptions<OTADbContext> options,
        //    ILogger<OTADbContext> logger = null) : base(options)
        //{
        //    _logger = logger;
        //}

        //public DbSet<OAPI_Authentication> Authentications { get; set; } // 인증
        //public DbSet<OAPI_Supplier> Suppliers { get; set; } // 공급사
        //public DbSet<OAPI_SyncClient> SyncClients { get; set; } // 싱크 클라이언트 (내부 혹은 외부 채널)

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        //    {
        //        foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        //    }

        //    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        //}
    }
}
