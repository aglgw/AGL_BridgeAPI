using AGL.Api.Domain.Entities.OAPI;
using Microsoft.EntityFrameworkCore;
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
    public class OAPI_DbContext : DbContext, IOAPIDbContext
    {
        public OAPI_DbContext(DbContextOptions<OAPI_DbContext> options) : base(options) { }

        public DbSet<OAPI_Supplier> Suppliers { get; set; }
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
