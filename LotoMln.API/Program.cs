using LotoMln.API.Extension;
using LotoMln.API.Hubs;
using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.Seed;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddPersistence(builder.Configuration, builder.Environment)
    .AddRepositories()
    .AddRedis(builder.Configuration)
    .AddGameServices()
    .AddRealtime(builder.Configuration)
    .AddCorsForDev()
    .AddControllers()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-seed questions on startup
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await QuestionSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors("Dev");
app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<GameHub>("/hubs/game");

app.Run();
