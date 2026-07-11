using CoparentHub.Api.Middleware;
using CoparentHub.Application;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Render/Heroku/Railway-style hosts assign a port at runtime via $PORT and expect the app to
// bind to it; local dev/docker-compose set ASPNETCORE_URLS explicitly instead, so PORT is
// simply absent there and this is a no-op.
var hostPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(hostPort))
{
    builder.WebHost.UseUrls($"http://+:{hostPort}");
}

builder.Host.UseSerilog((context, configuration) =>
{
    var seqUrl = context.Configuration["Seq:ServerUrl"];
    if (string.IsNullOrWhiteSpace(seqUrl)) seqUrl = "http://localhost:5341";

    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(seqUrl);
});

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
{
    if (builder.Environment.IsDevelopment())
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        jwtSecret = BitConverter.ToString(bytes).Replace("-", string.Empty);
        Console.WriteLine("Warning: Jwt:Secret was not set or was too short. A temporary development secret was generated.");
    }
    else
    {
        throw new InvalidOperationException(
            "Configuration error: Jwt:Secret must be set and at least 32 bytes (256 bits) long. " +
            "Set it via environment variable Jwt__Secret or `dotnet user-secrets` — never commit it to source control.");
    }
}
if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    throw new InvalidOperationException(
        "Configuration error: ConnectionStrings:DefaultConnection is not set.");
}

var encryptionKey = builder.Configuration["Encryption:Key"];
if (string.IsNullOrWhiteSpace(encryptionKey))
{
    throw new InvalidOperationException(
        "Configuration error: Encryption:Key is not set. Generate one with " +
        "`openssl rand -base64 32` and set it via the Encryption__Key environment " +
        "variable or `dotnet user-secrets` — never commit a production key to source " +
        "control. See SECURITY.md.");
}
try
{
    if (Convert.FromBase64String(encryptionKey).Length != 32)
    {
        throw new InvalidOperationException(
            "Configuration error: Encryption:Key must decode to exactly 32 bytes (256 bits).");
    }
}
catch (FormatException)
{
    throw new InvalidOperationException(
        "Configuration error: Encryption:Key must be a valid base64-encoded string.");
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditLoggingBehavior<,>));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<CoparentHub.Infrastructure.BackgroundJobs.EventReminderBackgroundService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .WithMethods("GET", "POST", "PUT", "DELETE")
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins([]);
        }
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
        o.IncludeErrorDetails = builder.Environment.IsDevelopment();
        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                var stampClaim = context.Principal?.FindFirstValue(JwtTokenService.SecurityStampClaimType);

                if (!Guid.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(stampClaim))
                {
                    context.Fail("Invalid token.");
                    return;
                }

                // Rejects tokens issued before a password reset — SecurityStamp is bumped on
                // every password change. GetByIdAsync is the cached path (10-min TTL), so this
                // is cheap.
                var userRepo = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var user = await userRepo.GetByIdAsync(userId, context.HttpContext.RequestAborted);

                if (user is null || !string.Equals(user.SecurityStamp.ToString(), stampClaim, StringComparison.Ordinal))
                {
                    context.Fail("Token has been revoked.");
                }
            },
            OnAuthenticationFailed = context =>
            {
                var log = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Security.Authentication");
                log.LogWarning(
                    "AUDIT action=JwtValidationFailed clientIp={ClientIp} path={Path} reason={Reason}",
                    context.HttpContext.Connection.RemoteIpAddress,
                    context.HttpContext.Request.Path,
                    context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var log = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Security.Authentication");
                log.LogInformation(
                    "AUDIT action=UnauthorizedRequest clientIp={ClientIp} path={Path}",
                    context.HttpContext.Connection.RemoteIpAddress,
                    context.HttpContext.Request.Path);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var authPermitLimit = builder.Configuration.GetValue("RateLimiting:AuthPermitLimit", 5);
var authWindowSeconds = builder.Configuration.GetValue("RateLimiting:AuthWindowSeconds", 60);
var globalPermitLimit = builder.Configuration.GetValue("RateLimiting:GlobalPermitLimit", 100);
var globalWindowSeconds = builder.Configuration.GetValue("RateLimiting:GlobalWindowSeconds", 60);
var messagesPermitLimit = builder.Configuration.GetValue("RateLimiting:MessagesPermitLimit", 20);
var messagesWindowSeconds = builder.Configuration.GetValue("RateLimiting:MessagesWindowSeconds", 60);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = authWindowSeconds.ToString();
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", ct);
    };

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authPermitLimit,
                Window = TimeSpan.FromSeconds(authWindowSeconds),
                QueueLimit = 0,
            }));

    options.AddPolicy("messages", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = messagesPermitLimit,
                Window = TimeSpan.FromSeconds(messagesWindowSeconds),
                QueueLimit = 0,
            }));

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = globalPermitLimit,
                Window = TimeSpan.FromSeconds(globalWindowSeconds),
                QueueLimit = 0,
            }));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 5 * 1024 * 1024;
});

// No file-upload endpoints exist, so a small cap here is safe for every legitimate request.
builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 1 * 1024 * 1024;
});

// Most PaaS hosts (Fly.io, Render, Railway, etc.) terminate TLS at their edge and forward
// plain HTTP to the container, so ASP.NET Core needs to trust their X-Forwarded-* headers to
// know the original request was HTTPS — otherwise UseHttpsRedirection/UseHsts below can loop.
// KnownNetworks/KnownProxies are cleared because the proxy's IP isn't fixed/known in advance
// on these platforms; this is the standard pattern for a containerized app behind a cloud
// provider's edge, not an open relay (only that provider's network can reach the container).
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoParent API", Version = "v1" });
        var scheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            In = ParameterLocation.Header,
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        };
        c.AddSecurityDefinition("Bearer", scheme);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, [] } });
    });
}

var app = builder.Build();

app.UseForwardedHeaders();

app.UseSerilogRequestLogging();

app.UseExceptionHandler(_ => { });

app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

// Applying migrations on startup is normally risky with multiple instances racing on the same
// migration, but this app only ever runs as a single instance (free-tier hosting doesn't
// support horizontal scaling anyway), so that risk doesn't apply — and it means there's no
// separate manual migration step to remember when deploying.
using (var migrationScope = app.Services.CreateScope())
{
    migrationScope.ServiceProvider
        .GetRequiredService<CoparentHub.Persistence.Data.AppDbContext>()
        .Database.Migrate();
}

app.UseHttpsRedirection();
app.UseSecurityHeaders();

app.UseCors("AllowFrontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
