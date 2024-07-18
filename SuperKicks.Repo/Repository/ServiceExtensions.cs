using Microsoft.Extensions.DependencyInjection;
using SuperKicks.Repo.Repository.Interface;

namespace SuperKicks.Repo.Repository
{
    public static class ServiceExtensions
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserManagerRepository, UserManagerRepository>();
        }
    }
}