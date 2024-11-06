using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
    public class OAPISupplierConfiguration : IEntityTypeConfiguration<OAPI_Supplier>
    {
        public void Configure(EntityTypeBuilder<OAPI_Supplier> builder)
        {
            builder.ToTable("OAPI_Supplier");
            builder.HasKey(e => e.SupplierId);
            builder.Property(e => e.FieldId).IsRequired().HasMaxLength(50);

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
        }
    }

    public class OAPIGolfClubConfiguration : IEntityTypeConfiguration<OAPI_GolfClub>
    {
        public void Configure(EntityTypeBuilder<OAPI_GolfClub> builder)
        {
            builder.ToTable("OAPI_GolfClub");
            builder.HasKey(e => e.GolfClubId);

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
        }
    }

    public class OAPIGolfClubImageConfiguration : IEntityTypeConfiguration<OAPI_GolfClubImage>
    {
        public void Configure(EntityTypeBuilder<OAPI_GolfClubImage> builder)
        {
            builder.ToTable("OAPI_GolfClubImage");
            builder.HasKey(e => e.GolfClubImageId);
        }
    }

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
        }
    }

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

        }
    }

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
        }
    }



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
                   .HasForeignKey(e => e.GolfClubId);

            builder.HasOne(e => e.GolfClubCourse)
                   .WithMany(c => c.TeeTimes)
                   .HasForeignKey(e => e.GolfClubCourseId);
        }
    }

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
                    .HasForeignKey(e => e.RefundPolicyId);
        }
    }


    public class OAPIDateSlotConfiguration : IEntityTypeConfiguration<OAPI_DateSlot>
    {
        public void Configure(EntityTypeBuilder<OAPI_DateSlot> builder)
        {
            builder.ToTable("OAPI_DateSlot");
            builder.HasKey(e => e.DateSlotId);
        }
    }

    public class OAPITimeSlotConfiguration : IEntityTypeConfiguration<OAPI_TimeSlot>
    {
        public void Configure(EntityTypeBuilder<OAPI_TimeSlot> builder)
        {
            builder.ToTable("OAPI_TimeSlot");
            builder.HasKey(e => e.TimeSlotId);
        }
    }

    public class OAPITeetimeRefundPolicyConfiguration : IEntityTypeConfiguration<OAPI_TeetimeRefundPolicy>
    {
        public void Configure(EntityTypeBuilder<OAPI_TeetimeRefundPolicy> builder)
        {
            builder.ToTable("OAPI_TeetimeRefundPolicy");
            builder.HasKey(e => e.RefundPolicyId);

            // OAPI_TeetimeRefundPolicy와 OAPI_TeetimeRefundMapping 간의 관계 설정
            builder.HasMany(rp => rp.TeeTimeMappings) // OAPI_TeetimeRefundPolicy는 여러 개의 OAPI_TeeTimeMapping과 관계를 가짐
                   .WithOne(tr => tr.TeetimeRefundPolicy) // 각 OAPI_TeeTimeMapping은 하나의 TeetimeRefundPolicy와 관계를 가짐
                   .HasForeignKey(tr => tr.RefundPolicyId) // 외래 키 설정
                   .OnDelete(DeleteBehavior.Restrict); // 삭제 동작을 제한함

            builder.Property(e => e.RefundUnit_1).HasColumnType("TINYINT");
            builder.Property(e => e.RefundUnit_2).HasColumnType("TINYINT");
            builder.Property(e => e.RefundUnit_3).HasColumnType("TINYINT");
            builder.Property(e => e.RefundUnit_4).HasColumnType("TINYINT");
            builder.Property(e => e.RefundUnit_5).HasColumnType("TINYINT");

            builder.Property(e => e.RefundFee_1).HasColumnType("decimal(18,4)");
            builder.Property(e => e.RefundFee_2).HasColumnType("decimal(18,4)");
            builder.Property(e => e.RefundFee_3).HasColumnType("decimal(18,4)");
            builder.Property(e => e.RefundFee_4).HasColumnType("decimal(18,4)");
            builder.Property(e => e.RefundFee_5).HasColumnType("decimal(18,4)");
            //builder.Property(e => e.RefundUnit_1)
            //        .HasConversion(
            //            v => (int?)v,         // byte? -> int?로 저장
            //            v => (byte?)v);       // int? -> byte?로 읽어오기
        }
    }

    public class OAPITeetimePricePolicyConfiguration : IEntityTypeConfiguration<OAPI_TeetimePricePolicy>
    {
        public void Configure(EntityTypeBuilder<OAPI_TeetimePricePolicy> builder)
        {
            builder.ToTable("OAPI_TeetimePricePolicy");
            builder.HasKey(e => e.PricePolicyId);

            // OAPI_TeetimeRefundPolicy와 OAPI_TeetimeRefundMapping 간의 관계 설정
            builder.HasMany(rp => rp.TeeTimeMappings) // OAPI_TeetimePricePolicy는 여러 개의 OAPI_TeeTimeMapping과 관계를 가짐
                   .WithOne(tr => tr.TeetimePricePolicy) // 각 OAPI_TeeTimeMapping은 하나의 TeetimePricePolicy와 관계를 가짐
                   .HasForeignKey(tr => tr.PricePolicyId) // 외래 키 설정
                   .OnDelete(DeleteBehavior.Restrict); // 삭제 동작을 제한함

            var decimalProperties = new[]
            {
                "GreenFee", "CartFee", "Tax", "AdditionalTax", "UnitPrice"
            };

            foreach (var property in decimalProperties)
            {
                for (int i = 1; i <= 6; i++)
                {
                    builder.Property(typeof(decimal?), $"{property}_{i}")
                        .HasColumnType("decimal(18,4)");
                }
            }
        }
    }

    public class OAPIReservationManagementConfiguration : IEntityTypeConfiguration<OAPI_ReservationManagement>
    {
        public void Configure(EntityTypeBuilder<OAPI_ReservationManagement> builder)
        {
            builder.ToTable("OAPI_ReservationManagement");
            builder.HasKey(e => e.ReservationManagementId);

            builder.HasOne(e => e.Supplier)
                   .WithMany(s => s.ReservationManagements)
                   .HasForeignKey(s => s.SupplierId);
            builder.Property(e => e.ReservationStatus).HasColumnType("TINYINT");
        }
    }

}
