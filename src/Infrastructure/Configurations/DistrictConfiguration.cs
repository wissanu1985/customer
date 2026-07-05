using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#nullable disable

namespace Infrastructure.Configurations;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> entity)
    {
        entity.HasKey(e => e.DistrictID).HasName("PK_Districts");
        entity.Property(e => e.DistrictID).ValueGeneratedNever();
        entity.Property(e => e.ProvinceID).IsRequired();
        entity.Property(e => e.DistrictThai).HasMaxLength(100).IsRequired();
        entity.Property(e => e.DistrictEng).HasMaxLength(100).IsRequired();
        entity.Property(e => e.DistrictThaiShort).HasMaxLength(100).IsRequired();
        entity.Property(e => e.DistrictEngShort).HasMaxLength(100).IsRequired();

        entity.HasMany(e => e.SubDistricts)
            .WithOne(s => s.District)
            .HasForeignKey(s => s.DistrictID)
            .HasConstraintName("FK_SubDistricts_Districts");
    }
}
