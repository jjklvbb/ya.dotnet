using Events.Api.Middlewares;
using Events.Application.Interfaces;
using Events.Application.Services;
using Events.Infrastructure;
using Events.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Введите JWT-токен без префикса Bearer"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

// Сервис мероприятий
builder.Services.AddScoped<IEventService, EventService>();

// Собственная база Events
builder.Services.AddEventsInfrastructure(
    builder.Configuration.GetConnectionString("EventsDb")
    ?? throw new InvalidOperationException(
        "Не настроено подключение к базе мероприятий."));

// Общая JWT-конфигурация
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Не настроен секрет JWT.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Не настроен издатель JWT.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Не настроена аудитория JWT.");

if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException(
        "Секрет JWT должен содержать минимум 32 байта.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = JwtRegisteredClaimNames.UniqueName,

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Применение миграций при старте
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<EventsDbContext>();

    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();