using System.Text;
using Mensajeria.Api.Datos;
using Mensajeria.Api.Modelos;
using Mensajeria.Api.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1. Registro de dependencias (Inyección de Dependencias) ----------

builder.Services.AddControllers();

// Base de datos SQLite (la ruta viene de ConnectionStrings:Mensajeria)
builder.Services.AddDbContext<MensajeriaDbContext>(opciones =>
    opciones.UseSqlite(builder.Configuration.GetConnectionString("Mensajeria")));

// Nuestros servicios. "Scoped" = una instancia nueva por cada petición HTTP.
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
builder.Services.AddScoped<IServicioMensajes, ServicioMensajes>();
builder.Services.AddScoped<IServicioJwt, ServicioJwt>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// ---------- 2. Autenticación con JWT ----------

var claveJwt = builder.Configuration["Jwt:Clave"];
if (string.IsNullOrWhiteSpace(claveJwt) || claveJwt.Length < 32)
    throw new InvalidOperationException(
        "Falta configurar 'Jwt:Clave' (mínimo 32 caracteres). Defínela con la variable de entorno Jwt__Clave.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false; // conservamos los nombres originales de los claims ("sub")
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Emisor"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// ---------- 3. Swagger con botón "Authorize" para el token ----------

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Mensajería",
        Version = "v1",
        Description = "Ejemplo educativo: ASP.NET Core + EF Core + SQLite + JWT."
    });

    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega aquí el token que devuelve POST /api/auth/login (sin la palabra 'Bearer')."
    });
    opciones.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = new List<string>()
    });

    // Muestra los comentarios /// de los controladores en Swagger
    opciones.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Mensajeria.Api.xml"));
});

var app = builder.Build();

// ---------- 4. Migraciones y datos iniciales al arrancar ----------

using (var alcance = app.Services.CreateScope())
{
    var db = alcance.ServiceProvider.GetRequiredService<MensajeriaDbContext>();
    var logger = alcance.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Si la ruta de la base apunta a una carpeta (p. ej. /datos), nos aseguramos de que exista
    var archivoBd = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(db.Database.GetConnectionString()).DataSource;
    var carpeta = Path.GetDirectoryName(Path.GetFullPath(archivoBd));
    if (carpeta is not null) Directory.CreateDirectory(carpeta);

    // Normalmente se aplican TODAS las migraciones pendientes. "BaseDeDatos:MigrarHasta" existe solo
    // para la demostración de run/demo-migraciones.sh (llegar hasta una migración concreta).
    var migrarHasta = app.Configuration["BaseDeDatos:MigrarHasta"];
    var pendientes = (await db.Database.GetPendingMigrationsAsync()).ToList();
    if (string.IsNullOrEmpty(migrarHasta))
        await db.Database.MigrateAsync();
    else
        await db.GetService<IMigrator>().MigrateAsync(migrarHasta);

    var aplicadas = await db.Database.GetAppliedMigrationsAsync();
    logger.LogInformation("Migraciones nuevas aplicadas al arrancar: {Pendientes}", pendientes.Count);
    logger.LogInformation("Migraciones en la base de datos: {Aplicadas}", string.Join(", ", aplicadas));

    if (app.Configuration.GetValue<bool>("DatosIniciales:Cargar"))
    {
        await DatosIniciales.CargarAsync(db, alcance.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>());
        logger.LogInformation("Usuarios de prueba listos (Alice y Bob, contraseña 123456).");
    }
}

// ---------- 5. Pipeline HTTP ----------

app.UseSwagger();
app.UseSwaggerUI(opciones => opciones.RoutePrefix = "swagger");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Necesario para que el proyecto de pruebas pueda arrancar la aplicación completa
public partial class Program { }
