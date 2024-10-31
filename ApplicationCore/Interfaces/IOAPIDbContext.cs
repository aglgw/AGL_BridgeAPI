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
        DbSet<OAPI_PricePolicy> PricePolicies { get; set; }
        DbSet<OAPI_TeetimeRefundPolicy> teetimeRefundPolicies { get; set; }
    }
    public class OAPI_DbContext_GetSupplier : DbContext, IOAPIDbContext
    {
        public OAPI_DbContext_GetSupplier(DbContextOptions<OAPI_DbContext_GetSupplier> options) : base(options) { }

        public DbSet<OAPI_Supplier> Suppliers { get; set; }
        public DbSet<OAPI_PricePolicy> PricePolicies { get; set; }
        public DbSet<OAPI_TeetimeRefundPolicy> teetimeRefundPolicies { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);

            builder.Entity<OAPI_Supplier>().ToTable("OAPI_Supplier");
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
