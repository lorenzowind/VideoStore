using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoStore.API.Models;

namespace VideoStore.API.Data.Mappings
{
    public class RentalMapping : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedOnAdd();

            builder.OwnsOne(r => r.RentalDate, tf =>
            {
                tf.Property(rentalDate => rentalDate.Date)
                    .IsRequired()
                    .HasColumnName("RentalDate")
                    .HasColumnType("datetime");
            });

            builder.OwnsOne(r => r.ReturnDate, tf =>
            {
                tf.Property(returnDate => returnDate.Date)
                    .IsRequired()
                    .HasColumnName("ReturnDate")
                    .HasColumnType("datetime");
            });

            builder.HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId);

            builder.HasOne(r => r.Movie)
                .WithMany()
                .HasForeignKey(r => r.MovieId);

            builder.HasIndex(r => r.CustomerId).HasDatabaseName("FK_Customer_idx");

            builder.HasIndex(r => r.MovieId).HasDatabaseName("FK_Movie_idx");

            builder.ToTable("Rentals");
        }
    }
}
