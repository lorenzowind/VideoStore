using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using VideoStore.API.Data;

namespace VideoStore.API.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptionsAction: 
                options =>
                    {
                        options.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    }));

            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("Total", builder =>
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                );
            });

            return services;
        }

        public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("Total");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            Task.Run(async () => {
                var maxAttempts = 12;
                var delay = 5000;

                using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();

                for (int i = 0; i < maxAttempts; i += 1)
                {
                    try
                    {
                        var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        if (!context.Database.GetAppliedMigrations().Any()) context.Database.Migrate();

                        return;
                    }
                    catch
                    {
                        await Task.Delay(delay);
                    }
                }
            }
);
            return app;
        }
    }
}
