using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#nullable disable

namespace Infrastructure.Configurations;

public class SubDistrictConfiguration : IEntityTypeConfiguration<SubDistrict>
{
    public void Configure(EntityTypeBuilder<SubDistrict> entity)
    {
        entity.HasKey(e => e.TambonID).HasName("PK_SubDistricts");
        entity.Property(e => e.TambonID).ValueGeneratedNever();
        entity.Property(e => e.DistrictID).IsRequired();
        entity.Property(e => e.TambonThai).HasMaxLength(200).IsRequired();
        entity.Property(e => e.TambonEng).HasMaxLength(200).IsRequired();
        entity.Property(e => e.TambonThaiShort).HasMaxLength(100).IsRequired();
        entity.Property(e => e.TambonEngShort).HasMaxLength(100).IsRequired();
        entity.Property(e => e.PostalCode).HasMaxLength(10).IsRequired();
    }
}
