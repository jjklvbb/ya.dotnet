using Bookings.Application.Ports;
using Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bookings.Infrastructure.Messaging;

namespace Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingsInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<BookingsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddSingleton<IBookingEventPublisher, KafkaBookingEventPublisher>();

        return services;
    }
}