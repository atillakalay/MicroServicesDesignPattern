using EventStore.ClientAPI;

namespace EventSourcing.API.EventStores
{
    public static class EventStoreExtensions
    {
        public static void AddEventStore(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("EventStore");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("EventStore connection string is not provided.");
            }

            IEventStoreConnection connection = EventStoreConnection.Create(connectionString);
            connection.ConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            services.AddSingleton(connection);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
            });

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Program");

            connection.Connected += (sender, args) =>
            {
                logger.LogInformation("EventStore connection established");
            };

            connection.ErrorOccurred += (sender, args) =>
            {
                logger.LogError(args.Exception, "EventStore connection error occurred");
            };
        }
    }
}
