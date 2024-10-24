using AGL.Api.Domain.Entities.OAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AGL.Api.ApplicationCore.Interfaces;

namespace AGL.Api.Infrastructure.Data
{
    public class OAPI_DbContext : DbContext
    {
        private readonly ILogger<OAPI_DbContext> _logger;

        public OAPI_DbContext(
            DbContextOptions<OAPI_DbContext> options,

            ILogger<OAPI_DbContext> logger = null) : base(options)
        {

            _logger = logger;
        }

        public DbSet<OAPI_Supplier> Suppliers { get; set; } // 공급사
        public DbSet<OAPI_GolfClub> GolfClubs { get; set; } // 골프장
        public DbSet<OAPI_GolfClubHole> Holes { get; set; } // 골프장홀
        public DbSet<OAPI_GolfClubRefundPolicy> GolfClubRefundPolicies { get; set; } // 골프장 환불정책
        public DbSet<OAPI_GolfClubImage> GolfClubImages { get; set; } // 골프장 이미지
        public DbSet<OAPI_GolfClubCourse> GolfClubCourses { get; set; } // 골프장 코스

        public DbSet<OAPI_DateSlot> DateSlots { get; set; } // 날짜
        public DbSet<OAPI_TimeSlot> TimeSlots { get; set; } // 시간
        public DbSet<OAPI_TeeTime> TeeTimes { get; set; } // 티타임
        public DbSet<OAPI_TeeTimeMapping> TeeTimeMappings { get; set; } // 티타임 날짜시간정보
        public DbSet<OAPI_TeetimePriceMapping> TeetimePriceMappings { get; set; } // 티타임 가격
        public DbSet<OAPI_TeetimeRefundMapping> TeetimeRefundMappings { get; set; } // 티타임 환불
        public DbSet<OAPI_TeetimeRefundPolicy> TeetimeRefundPolicies { get; set; } // 환불 정책
        public DbSet<OAPI_PricePolicy> PricePolicies { get; set; } // 가격 정책

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // OAPI_GolfClub
            builder.Entity<OAPI_GolfClub>()
                .HasOne(g => g.Supplier)
                .WithMany(s => s.GolfClubs)
                .HasForeignKey(g => g.SupplierId);

            // OAPI_GolfClubImage
            builder.Entity<OAPI_GolfClubImage>()
                .HasOne(i => i.GolfClub)
                .WithMany(g => g.GolfClubImages)
                .HasForeignKey(i => i.GolfClubId);

            // OAPI_GolfClubRefundPolicy
            builder.Entity<OAPI_GolfClubRefundPolicy>()
                .HasOne(r => r.GolfClub)
                .WithMany(g => g.RefundPolicies)
                .HasForeignKey(r => r.GolfClubId);

            // OAPI_GolfClubCourse
            builder.Entity<OAPI_GolfClubCourse>()
                .HasOne(c => c.GolfClub)
                .WithMany(g => g.Courses)
                .HasForeignKey(c => c.GolfClubId);

            // OAPI_GolfClubHole
            builder.Entity<OAPI_GolfClubHole>()
                .HasOne(h => h.GolfClub)
                .WithMany(g => g.Holes)
                .HasForeignKey(h => h.GolfClubId);

            // OAPI_TeetimePriceMapping
            builder.Entity<OAPI_TeetimePriceMapping>()
                .HasOne(tp => tp.DateTimeMapping)
                .WithMany(dm => dm.TeetimePriceMappings)
                .HasForeignKey(tp => tp.TeeTimeMappingId);

            builder.Entity<OAPI_TeetimePriceMapping>()
                .HasOne(tp => tp.PricePolicy)
                .WithMany(p => p.TeetimePriceMappings)
                .HasForeignKey(tp => tp.PricePolicyId);

            builder.Entity<OAPI_TeetimePriceMapping>()
                .HasKey(t => new { t.TeeTimeMappingId, t.PricePolicyId });

            // OAPI_TeetimeRefundMapping
            builder.Entity<OAPI_TeetimeRefundMapping>()
                .HasKey(t => new { t.TeeTimeMappingId, t.RefundPolicyId });

            builder.Entity<OAPI_TeetimeRefundMapping>()
                .HasOne(tr => tr.DateTimeMapping)
                .WithMany(dm => dm.TeetimeRefundMappings)
                .HasForeignKey(tr => tr.TeeTimeMappingId);

            builder.Entity<OAPI_TeetimeRefundMapping>()
                .HasOne(tr => tr.TeetimeRefundPolicy)
                .WithMany(tp => tp.TeetimeRefundMappings)
                .HasForeignKey(tr => tr.RefundPolicyId);

            // OAPI_DateTimeMapping
            builder.Entity<OAPI_TeeTimeMapping>()
                .HasOne(d => d.TeeTime)
                .WithMany(t => t.TeeTimeMappings)
                .HasForeignKey(d => d.TeetimeId);

            builder.Entity<OAPI_TeeTimeMapping>()
                .HasOne(d => d.DateSlot)
                .WithMany(ds => ds.TeeTimeMappings)
                .HasForeignKey(d => d.DateSlotId);

            builder.Entity<OAPI_TeeTimeMapping>()
                .HasOne(d => d.TimeSlot)
                .WithMany(ts => ts.TeeTimeMappings)
                .HasForeignKey(d => d.TimeSlotId);

            // OAPI_PricePolicy
            builder.Entity<OAPI_PricePolicy>()
                .HasMany(p => p.TeetimePriceMappings)
                .WithOne(tp => tp.PricePolicy)
                .HasForeignKey(tp => tp.PricePolicyId);

            // OAPI_TeetimeRefundPolicy
            builder.Entity<OAPI_TeetimeRefundPolicy>()
                .HasMany(t => t.TeetimeRefundMappings)
                .WithOne(tr => tr.TeetimeRefundPolicy)
                .HasForeignKey(tr => tr.RefundPolicyId);

        }
    }
}