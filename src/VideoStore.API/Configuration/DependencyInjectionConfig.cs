using Microsoft.Extensions.DependencyInjection;
using VideoStore.API.Data;
using VideoStore.API.Data.Repositories;
using VideoStore.API.Models;
using VideoStore.API.Services;

namespace VideoStore.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<IRentalService, RentalService>();

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IMovieRepository, MovieRepository>();
            services.AddScoped<IRentalRepository, RentalRepository>();

            services.AddScoped<ApplicationDbContext>();
        }
    }
}
