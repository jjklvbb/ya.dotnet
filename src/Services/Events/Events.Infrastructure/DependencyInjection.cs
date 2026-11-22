
using Events.Application.Ports;
using Events.Infrastructure.Caching;
using Events.Infrastructure.DataAccess;
using Events.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Events.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventsInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        services.AddDbContext<EventsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IEventRepository, EventRepository>();

        // Одно подключение Redis на всё время работы приложения
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisConnectionString =
                configuration["Redis:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Не настроено подключение к Redis.");

            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 3000;
            options.AsyncTimeout = 3000;

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        services.AddHostedService<KafkaTopicInitializer>();
        services.AddHostedService<BookingConfirmedConsumer>();

        return services;
    }
}
