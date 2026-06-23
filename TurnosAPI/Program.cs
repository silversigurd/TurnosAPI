using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TurnosAPI.Data;
using TurnosAPI.Exceptions;
using TurnosAPI.Repositories;
using TurnosAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "TurnosAPI",
        Version     = "v1",
        Description = "API para gestión de turnos — Portfolio Project (.NET 8 + EF Core + Stored Procedures)"
    });
});

builder.Services.AddDbContext<TurnosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Scoped: una instancia por request HTTP, compartida entre repositorios y servicios del mismo request
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProfesionalRepository, ProfesionalRepository>();
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();

builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProfesionalService, ProfesionalService>();
builder.Services.AddScoped<ITurnoService, TurnoService>();

var app = builder.Build();

// Manejo centralizado de excepciones: convierte las excepciones de dominio
// en respuestas HTTP sin necesidad de try/catch en cada controlador
app.UseExceptionHandler(configure =>
{
    configure.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.ContentType = "application/json";

        (context.Response.StatusCode, var mensaje) = exception switch
        {
            NotFoundException ex     => (StatusCodes.Status404NotFound,            ex.Message),
            ConflictException ex     => (StatusCodes.Status409Conflict,            ex.Message),
            BusinessRuleException ex => (StatusCodes.Status400BadRequest,          ex.Message),
            _                        => (StatusCodes.Status500InternalServerError,  "Error interno del servidor.")
        };

        await context.Response.WriteAsJsonAsync(new { error = mensaje });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TurnosAPI v1");
        c.RoutePrefix = "swagger";
    });

    // Al iniciar crea la base de datos, las tablas y los stored procedures si no existen
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TurnosDbContext>();
    await DbInitializer.InicializarAsync(db);
}

app.UseAuthorization();
app.MapControllers();
app.Run();
