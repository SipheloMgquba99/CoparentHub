using CoparentHub.Application.Features.Auth;
using CoparentHub.Application.Interfaces;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using CoparentHub.Infrastructure.Caching;
using CoparentHub.Infrastructure.Repositories;
using CoparentHub.Infrastructure.Security;
using CoparentHub.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
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
            services.AddSingleton<IFieldEncryptor, AesGcmFieldEncryptor>();

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(config.GetConnectionString("DefaultConnection"))
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

            services.AddMemoryCache();
            services.AddSingleton<IEventCacheVersion, EventCacheVersion>();

            services.AddScoped<UserRepository>();
            services.AddScoped<IUserRepository>(sp =>
                new CachedUserRepository(sp.GetRequiredService<UserRepository>(), sp.GetRequiredService<IMemoryCache>()));

            services.AddScoped<EventRepository>();
            services.AddScoped<IEventRepository>(sp =>
                new CachedEventRepository(
                    sp.GetRequiredService<EventRepository>(),
                    sp.GetRequiredService<IMemoryCache>(),
                    sp.GetRequiredService<IEventCacheVersion>()));

            services.AddScoped<IFamilyRepository, FamilyRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IFamilyInviteRepository, FamilyInviteRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

            return services;
        }
    }
}