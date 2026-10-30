using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TareaApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=tareas.db";
builder.Services.AddDbContext<TareaContext>(options => options.UseSqlite(connectionString));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Aplica las migraciones pendientes al arrancar
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<TareaContext>().Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
