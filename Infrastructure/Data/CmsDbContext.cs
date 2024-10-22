using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;


namespace AGL.Api.Infrastructure.Data
{
    public class CmsDbContext : DbContext
    {
        
        private readonly ILogger<CmsDbContext> _logger;
        public CmsDbContext(
            DbContextOptions<CmsDbContext> options,
            
            ILogger<CmsDbContext> logger = null) : base(options)
        {
            
            _logger = logger;
        }

        public DbSet<TA_Checkin_TeeTime> TA_Checkin_TeeTime { get; set; }
    

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
