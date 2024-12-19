using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AGL.Api.Domain.Entities.OTA;

namespace AGL.API.Infrastructure.Data.Configuration.OTA
{
    #region OTA_Authentication
    public class OTAAuthenticationConfiguration : IEntityTypeConfiguration<OTA_Authentication>
    {
        public void Configure(EntityTypeBuilder<OTA_Authentication> builder)
        {
            builder.ToTable("OAPI_Authentication_OTA");
            builder.HasKey(e => e.AuthenticationId);

            // SyncClient와의 관계 설정
            builder.HasOne(e => e.SyncClient)
                   .WithOne(c => c.Authentication) // SyncClient와의 양방향 관계
                   .HasForeignKey<OTA_Authentication>(e => e.SyncClientId) // 외래 키 지정
                   .IsRequired(false); // 선택적 관계
        }
    }
    #endregion

    #region OTA_SyncClient
    public class OTASyncClientConfiguration : IEntityTypeConfiguration<OTA_SyncClient>
    {
        public void Configure(EntityTypeBuilder<OTA_SyncClient> builder)
        {
            builder.ToTable("OAPI_SyncClient_OTA");
            builder.HasKey(e => e.SyncClientId);

            // Authentication 관계 (1:1, 선택적)
            builder.HasOne(e => e.Authentication)
                   .WithOne(a => a.SyncClient)
                   .HasForeignKey<OTA_Authentication>(a => a.SyncClientId) // 외래 키 지정
                   .IsRequired(false); // 선택적 관계

            builder.Property(e => e.IsSyncEnabled).HasColumnType("bit");
        }
    }
    #endregion
}
