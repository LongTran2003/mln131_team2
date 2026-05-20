using LotoMln.DataAccess.DBContext;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.API.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration cfg,
        IWebHostEnvironment env)
    {
        var connString = cfg.GetConnectionString("PostgreSqlConnection")
            ?? throw new InvalidOperationException(
                "Postgres connection string not found. Set via User Secrets.");

        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(connString, npg =>
            {
                npg.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
            });

            if (env.IsDevelopment())
            {
                opt.EnableDetailedErrors();
                opt.EnableSensitiveDataLogging();
            }
        });

        return services;
    }
}