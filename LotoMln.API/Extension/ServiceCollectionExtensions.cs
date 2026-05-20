using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.DataAccess.Repositories;
using LotoMln.Services.IServices;
using LotoMln.Services.Mapping;
using LotoMln.Services.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

        // Build NpgsqlDataSource với dynamic JSON enabled.
        // Cần thiết để serialize int[][], List<int>, string[] sang cột jsonb.
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(dataSource, npg =>
                npg.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));

            if (env.IsDevelopment())
            {
                opt.EnableDetailedErrors();
                opt.EnableSensitiveDataLogging();
            }
        });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IQuestionSlotRepository, QuestionSlotRepository>();
        services.AddScoped<IGameStateRepository, GameStateRepository>();
        services.AddScoped<ICalledNumberRepository, CalledNumberRepository>();
        services.AddScoped<IStealAttemptRepository, StealAttemptRepository>();
        services.AddScoped<IKinhClaimRepository, KinhClaimRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<ICardGeneratorService, CardGeneratorService>();

        // AutoMapper: scan assembly chứa GameMappingProfile
        services.AddAutoMapper(typeof(GameMappingProfile).Assembly);

        return services;
    }
}