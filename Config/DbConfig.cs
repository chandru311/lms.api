//using lms.api.Data;
//using Microsoft.EntityFrameworkCore;

//namespace cex.web.api.systemadmin.Configuration
//{
//    public static class DbConfig
//    {
//        public static IServiceCollection AddEntityFramework(this IServiceCollection services, IConfiguration configuration)
//        {
//            var connectionString = configuration.GetConnectionString("DefaultConnection");
//            services.AddDbContext<ApplicationDbContext>(options =>
//                options.UseSqlServer(connectionString, sqlServerOptions =>
//                {
//                    sqlServerOptions.EnableRetryOnFailure(
//                        maxRetryCount: 10,
//                        maxRetryDelay: TimeSpan.FromSeconds(30),
//                        errorNumbersToAdd: null);
//                }
//            ));

//            return services;
//        }
//    }
//}

using lms.api.Data;
using Microsoft.EntityFrameworkCore;

namespace cex.web.api.systemadmin.Configuration
{
    public static class EntityFrameworkConfiguration
    {
        public static IServiceCollection AddEntityFramework(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }
                ));
            return services;
        }
    }
}

