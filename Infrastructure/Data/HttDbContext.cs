using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;


namespace AGL.Api.Infrastructure.Data
{
    public class HttDbContext : DbContext
    {
        
        private readonly ILogger<HttDbContext> _logger;
        public HttDbContext(
            DbContextOptions<HttDbContext> options,
            
            ILogger<HttDbContext> logger = null) : base(options)
        {
            
            _logger = logger;
        }

        public DbSet<HTT_CURRENCY_CODE> HTT_CURRENCY_CODE { get; set; }

        public DbSet<HTT_CODE_NATION> HTT_CODE_NATION { get; set; }
        public DbSet<HTT_CODE_AREA> HTT_CODE_AREA { get; set; }
        public DbSet<HTT_CODE_CITY> HTT_CODE_CITY { get; set; }
        public DbSet<HTT_CODE_STATE> HTT_CODE_STATE { get; set; }



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
