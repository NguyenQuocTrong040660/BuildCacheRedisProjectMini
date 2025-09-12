using StackExchange.Redis;

namespace BuildCacheRedisProjectMini.Services
{
    public static class ServiceCollectionExtensions
    {

   
        public static void ConfigureApplicationServices(this IServiceCollection services,WebApplicationBuilder builder)
        {

            // Có thể dùng builder.Configuration để lấy các cấu hình
            var redisConfiguration = builder.Configuration.GetSection("Redis")["ConnectionString"];


            ConnectionMultiplexer redisIntanceConnection = ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints =
                {
                    redisConfiguration
                }
            });
            services.AddSingleton<IConnectionMultiplexer>(redisIntanceConnection);
            services.AddSingleton<IDatabase>(redisIntanceConnection.GetDatabase(-1, (object)null));

            builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

        }
    
    }
}
