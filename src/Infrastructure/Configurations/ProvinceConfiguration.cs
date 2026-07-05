using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#nullable disable

namespace Infrastructure.Configurations;

public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> entity)
    {
        entity.HasKey(e => e.ProvinceID).HasName("PK_Provinces");
        entity.Property(e => e.ProvinceID).ValueGeneratedNever();
        entity.Property(e => e.ProvinceThai).HasMaxLength(100).IsRequired();
        entity.Property(e => e.ProvinceEng).HasMaxLength(100).IsRequired();

        entity.HasMany(e => e.Districts)
            .WithOne(d => d.Province)
            .HasForeignKey(d => d.ProvinceID)
            .HasConstraintName("FK_Districts_Provinces");
    }
}
