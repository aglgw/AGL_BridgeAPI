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
}
