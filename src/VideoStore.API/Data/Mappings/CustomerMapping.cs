using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoStore.API.Models;
using VideoStore.Core.Domain;

namespace VideoStore.API.Data.Mappings
{
    public class CustomerMapping : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasColumnType("varchar(200)");

            builder.OwnsOne(c => c.Cpf, tf =>
            {
                tf.Property(cpf => cpf.Number)
                    .IsRequired()
                    .HasMaxLength(Cpf.MaxLength)
                    .HasColumnName("CPF")
                    .HasColumnType($"varchar({Cpf.MaxLength})");

                tf.HasIndex(cpf => cpf.Number).HasDatabaseName("idx_CPF");
            });

            builder.OwnsOne(c => c.BirthDate, tf =>
            {
                tf.Property(birthDate => birthDate.Date)
                    .IsRequired()
                    .HasColumnName("BirthDate")
                    .HasColumnType("datetime");
            });

            builder.HasIndex(c => c.Name).HasDatabaseName("idx_Name");

            builder.ToTable("Customers");
        }
    }
}
