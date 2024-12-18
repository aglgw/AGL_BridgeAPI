using AGL.Api.Domain.Entities.OAPI;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.API.Infrastructure.Data.Configuration.OTA
{
    //#region OAPI_Authentication
    //public class OAPIAuthenticationConfiguration : IEntityTypeConfiguration<OAPI_Authentication>
    //{
    //    public void Configure(EntityTypeBuilder<OAPI_Authentication> builder)
    //    {
    //        builder.ToTable("OAPI_Authentication");
    //        builder.HasKey(e => e.AuthenticationId);

    //        // Supplier 관계 설정 (선택적 관계)
    //        builder.HasOne(e => e.Supplier)
    //               .WithOne() // 역방향 네비게이션이 없는 경우
    //               .IsRequired(false); // 관계를 선택적으로 설정 (nullable)

    //        // SyncClient 관계 설정 (선택적 관계)
    //        builder.HasOne(e => e.SyncClient)
    //               .WithOne() // 역방향 네비게이션이 없는 경우
    //               .IsRequired(false); // 관계를 선택적으로 설정 (nullable)
    //    }
    //}
    //#endregion

    //#region OAPI_Supplier
    //public class OAPISupplierConfiguration : IEntityTypeConfiguration<OAPI_Supplier>
    //{
    //    public void Configure(EntityTypeBuilder<OAPI_Supplier> builder)
    //    {
    //        builder.ToTable("OAPI_Supplier");
    //        builder.HasKey(e => e.SupplierId);

    //        // Authentication 관계 (1:1, 선택적)
    //        builder.HasOne(e => e.Authentication)
    //               .WithOne(a => a.Supplier)
    //               .HasForeignKey<OAPI_Authentication>(a => a.SupplierId)
    //               .IsRequired(false); // 선택적 관계

    //        builder.Ignore(e => e.GolfClubs);
    //        builder.Ignore(e => e.ReservationManagements);
    //        builder.Ignore(e => e.TeeTimes);
    //    }
    //}
    //#endregion

    //#region OAPI_SyncClient
    //public class OAPISyncClientConfiguration : IEntityTypeConfiguration<OAPI_SyncClient>
    //{
    //    public void Configure(EntityTypeBuilder<OAPI_SyncClient> builder)
    //    {
    //        builder.ToTable("OAPI_SyncClient");
    //        builder.HasKey(e => e.SyncClientId);

    //        // Authentication 관계 (1:1, 선택적)
    //        builder.HasOne(e => e.Authentication)
    //               .WithOne(a => a.SyncClient)
    //               .HasForeignKey<OAPI_Authentication>(a => a.SyncClientId)
    //               .IsRequired(false); // 선택적 관계

    //        builder.Property(e => e.IsSyncEnabled).HasColumnType("bit");
    //    }
    //}
    //#endregion
}
