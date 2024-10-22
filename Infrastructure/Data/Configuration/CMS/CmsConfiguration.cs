using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AGL.Api.Domain.Entities;

namespace AGL.Api.Infrastructure.Data.Configuration
{
    class CheckinTeeTimeConfiguration : IEntityTypeConfiguration<TA_Checkin_TeeTime>
    {
        public void Configure(EntityTypeBuilder<TA_Checkin_TeeTime> builder)
        {
            builder.ToTable("TA_Checkin_TeeTime");
            builder.HasKey(e => new { e.TimeId }).HasName("PK_TA_Checkin_TeeTime");
            
        }
    }

    
}
