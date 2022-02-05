using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoStore.API.Models;

namespace VideoStore.API.Data.Mappings
{
    public class MovieMapping : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .ValueGeneratedOnAdd();

            builder.Property(m => m.Title)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.Property(m => m.ParentalRating)
                .HasColumnType("int");

            builder.Property(m => m.Launch)
                .HasColumnType("tinyint");

            builder.HasIndex(m => m.Launch).HasDatabaseName("idx_Launch");

            builder.HasIndex(m => m.Title).HasDatabaseName("idx_Title");

            builder.ToTable("Movies");
        }
    }
}
