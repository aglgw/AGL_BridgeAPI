using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AGL.Api.Domain.Entities;

namespace AGL.Api.Infrastructure.Data.Configuration
{
    class CURRENCYCODEConfiguration : IEntityTypeConfiguration<HTT_CURRENCY_CODE>
    {
        public void Configure(EntityTypeBuilder<HTT_CURRENCY_CODE> builder)
        {
            builder.ToTable("HTT_CURRENCY_CODE");
            builder.HasKey(e => new { e.CURRENCY_CODE }).HasName("PK_HTT_CURRENCY_CODE");

        }
    }

    class HTTCODENATIONConfiguration : IEntityTypeConfiguration<HTT_CODE_NATION>
    {
        public void Configure(EntityTypeBuilder<HTT_CODE_NATION> builder)
        {
            builder.ToTable("HTT_CODE_NATION");
            builder.HasKey(e => new { e.NAT_SEQ, e.NAT_CD }).HasName("PK_HTT_CODE_NATION");

        }
    }

    class HTTCODEAREAConfiguration : IEntityTypeConfiguration<HTT_CODE_AREA>
    {
        public void Configure(EntityTypeBuilder<HTT_CODE_AREA> builder)
        {
            builder.ToTable("HTT_CODE_AREA");
            builder.HasKey(e => new { e.AREA_CD }).HasName("PK_HTT_CODE_AREA");

        }
    }

    class HTTCODECITYConfiguration : IEntityTypeConfiguration<HTT_CODE_CITY>
    {
        public void Configure(EntityTypeBuilder<HTT_CODE_CITY> builder)
        {
            builder.ToTable("HTT_CODE_CITY");
            builder.HasKey(e => new { e.CITY_SEQ, e.CITY_CODE, e.NAT_CD }).HasName("PK_HTT_CODE_CITY");

        }
    }

    class HTTCODESTATEConfiguration : IEntityTypeConfiguration<HTT_CODE_STATE>
    {
        public void Configure(EntityTypeBuilder<HTT_CODE_STATE> builder)
        {
            builder.ToTable("HTT_CODE_STATE");
            builder.HasKey(e => new { e.STATE_SEQ }).HasName("PK_HTT_CODE_STATE");

        }
    }



}
