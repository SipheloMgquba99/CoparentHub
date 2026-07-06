namespace CoparentHub.Api.Middleware
{
    /// <summary>
    /// Adds defense-in-depth HTTP response headers recommended by OWASP
    /// (Secure Headers project) for an API that is consumed by a browser SPA.
    /// </summary>
    public static class SecurityHeadersMiddleware
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;

                headers["X-Content-Type-Options"] = "nosniff";

                headers["X-Frame-Options"] = "DENY";

                headers["Referrer-Policy"] = "no-referrer";

                headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

                headers["Permissions-Policy"] =
                    "geolocation=(), microphone=(), camera=(), payment=(), usb=()";

                headers.Remove("Server");
                headers.Remove("X-Powered-By");

                await next();
            });
        }
    }
}