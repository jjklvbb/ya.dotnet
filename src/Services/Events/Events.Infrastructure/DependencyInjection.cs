using Events.Application.Ports;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Events.Infrastructure.Messaging;

namespace Events.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventsInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<EventsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IEventRepository, EventRepository>();

        services.AddHostedService<KafkaTopicInitializer>();
        services.AddHostedService<BookingConfirmedConsumer>();

        return services;
    }
}