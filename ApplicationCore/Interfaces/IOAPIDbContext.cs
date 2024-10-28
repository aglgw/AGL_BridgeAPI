using AGL.Api.Domain.Entities.OAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Interfaces
{
    public interface IOAPIDbContext
    {
        DbSet<OAPI_Supplier> Suppliers { get; set; }
        DbSet<OAPI_TeetimePriceMapping> TeetimePriceMappings { get; set; }
        DbSet<OAPI_PricePolicy> PricePolicies { get; set; }
        DbSet<OAPI_TeetimeRefundMapping> TeetimeRefundMappings { get; set; }
        DbSet<OAPI_TeetimeRefundPolicy> teetimeRefundPolicies { get; set; }
    }
    public class OAPI_DbContext_GetSupplier : DbContext, IOAPIDbContext
    {
        public OAPI_DbContext_GetSupplier(DbContextOptions<OAPI_DbContext_GetSupplier> options) : base(options) { }

        public DbSet<OAPI_Supplier> Suppliers { get; set; }
        public DbSet<OAPI_TeetimePriceMapping> TeetimePriceMappings { get; set; }
        public DbSet<OAPI_PricePolicy> PricePolicies { get; set; }
        public DbSet<OAPI_TeetimeRefundMapping> TeetimeRefundMappings { get; set; }
        public DbSet<OAPI_TeetimeRefundPolicy> teetimeRefundPolicies { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);

            builder.Entity<OAPI_Supplier>().ToTable("OAPI_Supplier");

            builder.Entity<OAPI_TeetimePriceMapping>().HasNoKey();
            builder.Entity<OAPI_TeetimeRefundMapping>().HasNoKey();

            builder.Entity<OAPI_TeetimePriceMapping>()
                .HasKey(tp => new { tp.TeeTimeMappingId, tp.PricePolicyId });
            builder.Entity<OAPI_TeetimeRefundMapping>()
                .HasKey(tp => new { tp.TeeTimeMappingId, tp.RefundPolicyId });

            // OAPI_TeetimePriceMapping과 OAPI_TeeTimeMapping의 관계 설정
            builder.Entity<OAPI_TeetimePriceMapping>()
                .HasOne(tp => tp.TeeTimeMapping)
                .WithMany(tt => tt.TeetimePriceMappings)
                .HasForeignKey(tp => tp.TeeTimeMappingId);

            // OAPI_TeetimePriceMapping과 OAPI_PricePolicy의 관계 설정
            builder.Entity<OAPI_TeetimePriceMapping>()
                .HasOne(tp => tp.PricePolicy)
                .WithMany(pp => pp.TeetimePriceMappings)
                .HasForeignKey(tp => tp.PricePolicyId);

            //// OAPI_TeeTimeMapping과 OAPI_TeetimeRefundMapping 간의 관계 설정
            builder.Entity<OAPI_TeetimeRefundMapping>()
                .HasOne(tr => tr.TeeTimeMapping)
                .WithMany(tt => tt.TeetimeRefundMappings)
                .HasForeignKey(tr => tr.TeeTimeMappingId);

            //// OAPI_TeeTimeMapping과 OAPI_TeetimeRefundPolicy 간의 관계 설정
            builder.Entity<OAPI_TeetimeRefundMapping>()
                .HasOne(tp => tp.TeetimeRefundPolicy)
                .WithMany(pp => pp.TeetimeRefundMappings)
                .HasForeignKey(tp => tp.RefundPolicyId);
        }
    }

    public interface IMyDatabaseService
    {
        Task<OAPI_Supplier> GetSupplierByCodeAsync(string supplierCode);
    }

    public class MyDatabaseService : IMyDatabaseService
    {
        private readonly IOAPIDbContext _context;

        public MyDatabaseService(IOAPIDbContext context)
        {
            _context = context;
        }

        public async Task<OAPI_Supplier> GetSupplierByCodeAsync(string supplierCode)
        {
            return await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
        }
    }
}
