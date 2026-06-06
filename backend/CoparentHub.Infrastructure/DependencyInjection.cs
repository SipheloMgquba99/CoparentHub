using CoparentHub.Application.Features.Auth;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Infrastructure.Repositories;
using CoparentHub.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoparentHub.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFamilyRepository, FamilyRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

            return services;
        }
    }
}