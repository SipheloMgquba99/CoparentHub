using MediatR;
using System.Diagnostics;
using System.Reflection;

namespace CoparentHub.Api.Middleware
{
    public class AuditLoggingBehavior<TRequest, TResponse>(
        ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor httpContextAccessor) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        // Covers both credential-shaped fields and free-text/PII-shaped ones (child/family
        // details, event notes, message bodies, etc.) — this app encrypts those at rest, so
        // the audit trail must not undo that by logging them in plaintext to Seq.
        private static readonly string[] SensitiveKeywords =
            ["password", "token", "secret", "hash", "authorization",
             "body", "message", "description", "notes", "reason",
             "allerg", "medicat", "doctor", "school", "contact", "address", "phone", "name"];

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            if (!requestName.EndsWith("Command", StringComparison.Ordinal))
                return await next();

            var http = httpContextAccessor.HttpContext;
            var actorId =
                http?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? http?.User?.FindFirst("sub")?.Value
                ?? "anonymous";
            var clientIp = http?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            var details = RedactSensitiveFields(request);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();
                stopwatch.Stop();

                var (success, error) = ExtractOutcome(response);

                logger.Log(
                    success ? LogLevel.Information : LogLevel.Warning,
                    "AUDIT action={Action} actor={ActorId} clientIp={ClientIp} outcome={Outcome} durationMs={DurationMs} error={Error} details={@Details}",
                    requestName,
                    actorId,
                    clientIp,
                    success ? "Success" : "Failure",
                    stopwatch.ElapsedMilliseconds,
                    error,
                    details);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                logger.LogError(
                    ex,
                    "AUDIT action={Action} actor={ActorId} clientIp={ClientIp} outcome=Exception durationMs={DurationMs} details={@Details}",
                    requestName,
                    actorId,
                    clientIp,
                    stopwatch.ElapsedMilliseconds,
                    details);

                throw;
            }
        }

        private static Dictionary<string, object?> RedactSensitiveFields(TRequest request)
        {
            var result = new Dictionary<string, object?>();

            foreach (var prop in typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                result[prop.Name] = SensitiveKeywords.Any(k =>
                    prop.Name.Contains(k, StringComparison.OrdinalIgnoreCase))
                    ? "[REDACTED]"
                    : SafeValue(prop.GetValue(request));
            }

            return result;
        }

        private static object? SafeValue(object? value) => value switch
        {
            null => null,
            string s => s,
            Guid or DateTime or DateOnly or bool or int or long or double or decimal => value,
            Enum e => e.ToString(),
            _ when value.GetType().IsPrimitive => value,
            _ => value.ToString(),
        };

        private static (bool success, string? error) ExtractOutcome(TResponse response)
        {
            if (response is null) return (true, null);

            var type = response.GetType();
            var isSuccessProp = type.GetProperty("IsSuccess");
            if (isSuccessProp is null) return (true, null);

            var isSuccess = (bool)(isSuccessProp.GetValue(response) ?? true);
            var error = type.GetProperty("Error")?.GetValue(response) as string;
            return (isSuccess, error);
        }
    }
}
