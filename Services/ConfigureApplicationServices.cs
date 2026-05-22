using StackExchange.Redis;

namespace BuildCacheRedisProjectMini.Services
{
    public static class ServiceCollectionExtensions
    {

   
        public static void ConfigureApplicationServices(this IServiceCollection services,WebApplicationBuilder builder)
        {

            // Có thể dùng builder.Configuration để lấy các cấu hình
            var redisConfiguration = builder.Configuration.GetSection("Redis")["ConnectionString"] ?? "localhost:6379";


            ConnectionMultiplexer redisIntanceConnection = ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints =
                {
                    redisConfiguration
                }
            });
            services.AddSingleton<IConnectionMultiplexer>(redisIntanceConnection);
            services.AddSingleton<IDatabase>(redisIntanceConnection.GetDatabase());

            services.AddScoped<IRedisCacheService, RedisCacheService>();

            services.AddHealthChecks()
                .AddAsyncCheck("Redis", async () =>
                {
                    try
                    {
                        var database = redisIntanceConnection.GetDatabase();
                        await database.PingAsync();
                        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Redis is healthy.");
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Redis connection failed.", ex);
                    }
                });

        }
    
    }
}
