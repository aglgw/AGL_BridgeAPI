﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AGL.Api.Domain.Entities.OAPI;
using AGL.Api.Infrastructure.Data;

namespace AGL.API.Infrastructure.Data.Configuration.OAPI
{
    #region OAPI_Authentication
    public class OAPIAuthenticationConfiguration : IEntityTypeConfiguration<OAPI_Authentication>
    {
        public void Configure(EntityTypeBuilder<OAPI_Authentication> builder)
        {
            builder.ToTable("OAPI_Authentication");
            builder.HasKey(e => e.AuthenticationId);

            // Supplier 관계 설정 (선택적 관계)
            builder.HasOne(e => e.Supplier)
                   .WithOne() // 역방향 네비게이션이 없는 경우
                   .IsRequired(false); // 관계를 선택적으로 설정 (nullable)

            // SyncClient 관계 설정 (선택적 관계)
            builder.HasOne(e => e.SyncClient)
                   .WithOne() // 역방향 네비게이션이 없는 경우
                   .IsRequired(false); // 관계를 선택적으로 설정 (nullable)
        }
    }
    #endregion

    #region OAPI_Supplier
    public class OAPISupplierConfiguration : IEntityTypeConfiguration<OAPI_Supplier>
    {
        public void Configure(EntityTypeBuilder<OAPI_Supplier> builder)
        {
            builder.ToTable("OAPI_Supplier");
            builder.HasKey(e => e.SupplierId);

            builder.HasMany(e => e.GolfClubs)
                   .WithOne(g => g.Supplier)
                   .HasForeignKey(g => g.SupplierId);
            //.OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.ReservationManagements)
                   .WithOne(m => m.Supplier)
                   .HasPrincipalKey(m => m.SupplierId);
            //.OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.TeeTimes)
                   .WithOne(t => t.Supplier)
                   .HasPrincipalKey(t => t.SupplierId);
            //.OnDelete(DeleteBehavior.Cascade);

            // Authentication 관계 (1:1, 선택적)
            builder.HasOne(e => e.Authentication)
                   .WithOne(a => a.Supplier)
                   .HasForeignKey<OAPI_Authentication>(a => a.SupplierId)
                   .IsRequired(false); // 선택적 관계
        }
    }
    #endregion

    #region OAPI_GolfClub
    public class OAPIGolfClubConfiguration : IEntityTypeConfiguration<OAPI_GolfClub>
    {
        public void Configure(EntityTypeBuilder<OAPI_GolfClub> builder)
        {
            builder.ToTable("OAPI_GolfClub");
            builder.HasKey(e => e.GolfClubId);

            builder.HasOne(e => e.Supplier)
                   .WithMany(s => s.GolfClubs)
                   .HasForeignKey(g => g.SupplierId);

            builder.HasMany(e => e.GolfClubImages)
                   .WithOne(g => g.GolfClub)
                   .HasForeignKey(g => g.GolfClubId);

            builder.HasMany(e => e.RefundPolicies)
                   .WithOne(r => r.GolfClub)
                   .HasForeignKey(r => r.GolfClubId);

            builder.HasMany(e => e.Courses)
                   .WithOne(c => c.GolfClub)
                   .HasForeignKey(c => c.GolfClubId);

            builder.HasMany(e => e.Holes)
                   .WithOne(h => h.GolfClub)
                   .HasForeignKey(h => h.GolfClubId);

            builder.Property(e => e.isGuestInfoRequired).HasColumnType("bit");

            // IsDeleted = 0 전역 필터 추가
            builder.HasQueryFilter(e => !e.IsDeleted);

            // 유니크 조건 추가
            builder.HasIndex(e => new { e.SupplierId, e.GolfClubCode })
                   .IsUnique()
                   .HasDatabaseName("UQ_SupplierId_GolfClubCode");
        }
    }
    #endregion

    #region OAPI_GolfClubImage
    public class OAPIGolfClubImageConfiguration : IEntityTypeConfiguration<OAPI_GolfClubImage>
    {
        public void Configure(EntityTypeBuilder<OAPI_GolfClubImage> builder)
        {
            builder.ToTable("OAPI_GolfClubImage");
            builder.HasKey(e => e.GolfClubImageId);

            // IsDeleted = 0 전역 필터 추가
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    #endregion

    #region OAPI_GolfClubRefundPolicy
    public class OAPIGolfClubRefundPolicyConfiguration : IEntityTypeConfiguration<OAPI_GolfClubRefundPolicy>
    {
        public void Configure(EntityTypeBuilder<OAPI_GolfClubRefundPolicy> builder)
        {
            builder.ToTable("OAPI_GolfClubRefundPolicy");
            builder.HasKey(e => e.GolfClubRefundPolicyId);

            builder.HasOne(r => r.GolfClub)
                   .WithMany(g => g.RefundPolicies)
                   .HasForeignKey(r => r.GolfClubId);

            builder.Property(e => e.RefundFee).HasColumnType("decimal(18,4)");
            builder.Property(e => e.RefundUnit).HasColumnType("TINYINT");

            // IsDeleted = 0 전역 필터 추가
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    #endregion

    #region OAPI_GolfClubCourse
    public class OAPIGolfClubCourseConfiguration : IEntityTypeConfiguration<OAPI_GolfClubCourse>
    {
        public void Configure(EntityTypeBuilder<OAPI_GolfClubCourse> builder)
        {
            builder.ToTable("OAPI_GolfClubCourse");
            builder.HasKey(e => e.GolfClubCourseId);

            builder.HasOne(c => c.GolfClub)
                   .WithMany(g => g.Courses)
                   .HasForeignKey(c => c.GolfClubId);

            builder.HasMany(e => e.TeeTimes)
                   .WithOne(c => c.GolfClubCourse)
                   .HasForeignKey(c => c.GolfClubCourseId);

            // IsDeleted = 0 전역 필터 추가
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    #endregion

    #region OAPI_GolfClubHole
    public class OAPIGolfClubHoleConfiguration : IEntityTypeConfiguration<OAPI_GolfClubHole>
    {
        public void Configure(EntityTypeBuilder<OAPI_GolfClubHole> builder)
        {
            builder.ToTable("OAPI_GolfClubHole");
            builder.HasKey(e => e.GolfClubHoleId);

            // OAPI_GolfClubHole과 OAPI_GolfClub 간의 관계 설정
            builder.HasOne(h => h.GolfClub) // 각 GolfClubHole은 하나의 GolfClub에 속함
                   .WithMany(g => g.Holes) // 각 GolfClub은 여러 개의 GolfClubHole을 가질 수 있음
                   .HasForeignKey(h => h.GolfClubId) // 외래 키 설정
                   .OnDelete(DeleteBehavior.Restrict); // 삭제 동작을 제한함

            // IsDeleted = 0 전역 필터 추가
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    #endregion

    #region OAPI_TeeTime
    public class OAPITeeTimeConfiguration : IEntityTypeConfiguration<OAPI_TeeTime>
    {
        public void Configure(EntityTypeBuilder<OAPI_TeeTime> builder)
        {
            builder.ToTable("OAPI_TeeTime");
            builder.HasKey(e => e.TeetimeId);

            builder.HasOne(e => e.Supplier)
                   .WithMany(s => s.TeeTimes)
                   .HasForeignKey(e => e.SupplierId);

            builder.HasOne(e => e.GolfClub)
                   .WithMany(g => g.TeeTimes)
                   .HasForeignKey(e => e.GolfClubId)
                   .IsRequired(false);

            builder.HasOne(e => e.GolfClubCourse)
                   .WithMany(c => c.TeeTimes)
                   .HasForeignKey(e => e.GolfClubCourseId);
        }
    }
    #endregion

    #region OAPI_TeeTimeMapping
    public class OAPITeeTimeMappingConfiguration : IEntityTypeConfiguration<OAPI_TeeTimeMapping>
    {
        public void Configure(EntityTypeBuilder<OAPI_TeeTimeMapping> builder)
        {
            builder.ToTable("OAPI_TeeTimeMapping");

            builder.HasOne(e => e.TeeTime)
                   .WithMany(t => t.TeeTimeMappings)
                   .HasForeignKey(e => e.TeetimeId);

            builder.HasOne(e => e.DateSlot)
                   .WithMany(d => d.TeeTimeMappings)
                   .HasForeignKey(e => e.DateSlotId);

            builder.HasOne(e => e.TimeSlot)
                   .WithMany(t => t.TeeTimeMappings)
                   .HasForeignKey(e => e.TimeSlotId);

            builder.HasOne(e => e.TeetimePricePolicy)
                    .WithMany(t => t.TeeTimeMappings)
                    .HasForeignKey(e => e.PricePolicyId);

            builder.HasOne(e => e.TeetimeRefundPolicy)
                    .WithMany(t => t.TeeTimeMappings)
                    .HasForeignKey(e => e.RefundPolicyId)
                    .IsRequired(false);

            // 유니크 조건 추가
            builder.HasIndex(e => new { e.TeetimeId, e.DateSlotId, e.TimeSlotId })
                   .IsUnique()
                   .HasDatabaseName("UQ_TeeTimeMapping");
        }
    }
    #endregion

    #region OAPI_DateSlot
    public class OAPIDateSlotConfiguration : IEntityTypeConfiguration<OAPI_DateSlot>
    {
        public void Configure(EntityTypeBuilder<OAPI_DateSlot> builder)
        {
            builder.ToTable("OAPI_DateSlot");
            builder.HasKey(e => e.DateSlotId);

            builder.Property(e => e.StartDate)
                   .HasColumnType("date"); // SQL Server의 DATE 형식
        }
    }
    #endregion

    #region OAPI_TimeSlot

    public class OAPITimeSlotConfiguration : IEntityTypeConfiguration<OAPI_TimeSlot>
    {
        public void Configure(EntityTypeBuilder<OAPI_TimeSlot> builder)
        {
            builder.ToTable("OAPI_TimeSlot");
            builder.HasKey(e => e.TimeSlotId);
        }
    }
    #endregion

    #region OAPI_TeetimeRefundPolicy
    public class OAPITeetimeRefundPolicyConfiguration : IEntityTypeConfiguration<OAPI_TeetimeRefundPolicy>
    {
        public void Configure(EntityTypeBuilder<OAPI_TeetimeRefundPolicy> builder)
        {
            builder.ToTable("OAPI_TeetimeRefundPolicy");
            builder.HasKey(e => e.RefundPolicyId);

            var decimalProperties = new[]
            {
                "RefundFee"
            };

            foreach (var property in decimalProperties)
            {
                for (int i = 1; i <= 5; i++)
                {
                    builder.Property(typeof(decimal?), $"{property}_{i}")
                        .HasColumnType("decimal(18,4)");
                }
            }

            var tinyintProperties = new[]
{
                "RefundUnit"
            };

            foreach (var property in tinyintProperties)
            {
                for (int i = 1; i <= 5; i++)
                {
                    builder.Property($"{property}_{i}").HasColumnType("TINYINT");
                }
            }

            // OAPI_TeetimeRefundPolicy와 OAPI_TeetimeRefundMapping 간의 관계 설정
            builder.HasMany(rp => rp.TeeTimeMappings) // OAPI_TeetimeRefundPolicy는 여러 개의 OAPI_TeeTimeMapping과 관계를 가짐
                   .WithOne(tr => tr.TeetimeRefundPolicy) // 각 OAPI_TeeTimeMapping은 하나의 TeetimeRefundPolicy와 관계를 가짐
                   .HasForeignKey(tr => tr.RefundPolicyId) // 외래 키 설정
                   .OnDelete(DeleteBehavior.Restrict); // 삭제 동작을 제한함
        }
    }
    #endregion

    #region OAPI_TeetimePricePolicy
    public class OAPITeetimePricePolicyConfiguration : IEntityTypeConfiguration<OAPI_TeetimePricePolicy>
    {
        public void Configure(EntityTypeBuilder<OAPI_TeetimePricePolicy> builder)
        {
            builder.ToTable("OAPI_TeetimePricePolicy");
            builder.HasKey(e => e.PricePolicyId);

            var decimalProperties = new[]
            {
                "GreenFee", "CartFee", "CaddyFee", "Tax", "AdditionalTax", "UnitPrice"
            };

            foreach (var property in decimalProperties)
            {
                for (int i = 1; i <= 6; i++)
                {
                    builder.Property(typeof(decimal?), $"{property}_{i}")
                        .HasColumnType("decimal(18,4)");
                }
            }

            // OAPI_TeetimeRefundPolicy와 OAPI_TeetimeRefundMapping 간의 관계 설정
            builder.HasMany(rp => rp.TeeTimeMappings) // OAPI_TeetimePricePolicy는 여러 개의 OAPI_TeeTimeMapping과 관계를 가짐
                   .WithOne(tr => tr.TeetimePricePolicy) // 각 OAPI_TeeTimeMapping은 하나의 TeetimePricePolicy와 관계를 가짐
                   .HasForeignKey(tr => tr.PricePolicyId) // 외래 키 설정
                   .OnDelete(DeleteBehavior.Restrict); // 삭제 동작을 제한함

        }
    }
    #endregion

    #region OAPI_ReservationManagement
    public class OAPIReservationManagementConfiguration : IEntityTypeConfiguration<OAPI_ReservationManagement>
    {
        public void Configure(EntityTypeBuilder<OAPI_ReservationManagement> builder)
        {
            builder.ToTable("OAPI_ReservationManagement");
            builder.HasKey(e => e.ReservationManagementId);

            var decimalProperties = new[] { "TotalPrice", "cancelPenaltyAmount" };

            foreach (var property in decimalProperties)
            {
                builder.Property(property).HasPrecision(18, 4);
            }

            builder.HasOne(e => e.Supplier)
                   .WithMany(s => s.ReservationManagements)
                   .HasForeignKey(s => s.SupplierId);
            
            builder.HasMany(e => e.Guests)
                   .WithOne(g => g.ReservationManagement)
                   .HasForeignKey(g => g.ReservationManagementId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.ReservationStatus).HasColumnType("TINYINT");
            builder.Property(e => e.ReservationMembers).HasColumnType("TINYINT");
        }
    }
    #endregion

    #region OAPI_ReservationmanagementGuest
    public class OAPIReservationManagementGuestConfiguration : IEntityTypeConfiguration<OAPI_ReservationmanagementGuest>
    {
        public void Configure(EntityTypeBuilder<OAPI_ReservationmanagementGuest> builder)
        {
            builder.ToTable("OAPI_ReservationmanagementGuest");
            builder.HasKey(e => e.ReservationManagementGuestId);

            builder.HasOne(g => g.ReservationManagement)
                   .WithMany(r => r.Guests)
                   .HasForeignKey(g => g.ReservationManagementId);

            builder.Property(e => e.Idx).HasColumnType("TINYINT");
        }
    }
    #endregion

    #region OAPI_SyncClient
    public class OAPISyncClientConfiguration : IEntityTypeConfiguration<OAPI_SyncClient>
    {
        public void Configure(EntityTypeBuilder<OAPI_SyncClient> builder)
        {
            builder.ToTable("OAPI_SyncClient");
            builder.HasKey(e => e.SyncClientId);

            // Authentication 관계 (1:1, 선택적)
            builder.HasOne(e => e.Authentication)
                   .WithOne(a => a.SyncClient)
                   .HasForeignKey<OAPI_Authentication>(a => a.SyncClientId)
                   .IsRequired(false); // 선택적 관계

            builder.Property(e => e.IsSyncEnabled).HasColumnType("bit");
        }
    }
    #endregion

    #region OAPI_SyncTeeTimeMapping
    public class OAPISyncTeeTimeMappingConfiguration : IEntityTypeConfiguration<OAPI_SyncTeeTimeMapping>
    {
        public void Configure(EntityTypeBuilder<OAPI_SyncTeeTimeMapping> builder)
        {
            builder.ToTable("OAPI_SyncTeeTimeMapping");
            builder.HasKey(e => e.SyncTeeTimeMappingId);

            builder.HasOne(e => e.TeeTimeMapping)
                   .WithOne()
                   .HasForeignKey<OAPI_SyncTeeTimeMapping>(stm => stm.SyncTeeTimeMappingId);
        }
    }
    #endregion
}
