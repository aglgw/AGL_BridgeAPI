﻿using AGL.Api.Domain.Entities.OAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AGL.Api.ApplicationCore.Interfaces;
using System.Reflection;

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
        public DbSet<OAPI_GolfClubHole> GolfClubHoles { get; set; } // 골프장홀
        public DbSet<OAPI_GolfClubRefundPolicy> GolfClubRefundPolicies { get; set; } // 골프장 환불정책
        public DbSet<OAPI_GolfClubImage> GolfClubImages { get; set; } // 골프장 이미지
        public DbSet<OAPI_GolfClubCourse> GolfClubCourses { get; set; } // 골프장 코스

        public DbSet<OAPI_DateSlot> DateSlots { get; set; } // 날짜
        public DbSet<OAPI_TimeSlot> TimeSlots { get; set; } // 시간
        public DbSet<OAPI_TeeTime> TeeTimes { get; set; } // 티타임
        public DbSet<OAPI_TeeTimeMapping> TeeTimeMappings { get; set; } // 티타임 날짜시간정보
        public DbSet<OAPI_TeetimeRefundPolicy> TeetimeRefundPolicies { get; set; } // 환불 정책
        public DbSet<OAPI_TeetimePricePolicy> TeetimePricePolicies { get; set; } // 가격 정책
        public DbSet<OAPI_ReservationManagement> ReservationManagements { get; set; } // 가격 정책

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}