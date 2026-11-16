using Bookings.Application.Interfaces;
using Bookings.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Bookings.Api.Middlewares;
using Bookings.Infrastructure;
using Bookings.Infrastructure.DataAccess;
using Bookings.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

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

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IBookingProcessingService, BookingProcessingService>();

builder.Services.AddHostedService<BookingBackgroundService>();

builder.Services.AddBookingsInfrastructure(
    builder.Configuration.GetConnectionString("BookingsDb")
    ?? throw new InvalidOperationException(
        "Не настроено подключение к базе бронирований."));

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<BookingsDbContext>();

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