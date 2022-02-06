using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VideoStore.API.Models;
using VideoStore.Core.Data;

namespace VideoStore.API.Data
{
    public class ApplicationDbContext : DbContext, IUnitOfWork
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Movie> Movies { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        public async Task<bool> Commit() => await base.SaveChangesAsync() > 0;
    }
}
