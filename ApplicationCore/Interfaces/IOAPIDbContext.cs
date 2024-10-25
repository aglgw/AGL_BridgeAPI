using AGL.Api.Domain.Entities.OAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Interfaces
{
    public interface IOAPIDbContext
    {
        DbSet<OAPI_Supplier> Suppliers { get; set; }
    }
    public class OAPI_DbContext_GetSupplier : DbContext, IOAPIDbContext
    {
        public OAPI_DbContext_GetSupplier(DbContextOptions<OAPI_DbContext_GetSupplier> options) : base(options) { }

        public DbSet<OAPI_Supplier> Suppliers { get; set; }
    }

    public interface IMyDatabaseService
    {
        Task<OAPI_Supplier> GetSupplierByCodeAsync(string supplierCode);
    }

    public class MyDatabaseService : IMyDatabaseService
    {
        private readonly IOAPIDbContext _context;
        private readonly string _connectionString;

        public MyDatabaseService(IOAPIDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("OAPI.Application.ConnectionString");
        }

        public async Task<OAPI_Supplier> GetSupplierByCodeAsync(string supplierCode)
        {
            return await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
        }
    }
}
