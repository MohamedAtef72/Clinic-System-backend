using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Clinic_System.API.Middleware
{
    /// <summary>
    /// Global exception-handling middleware.
    ///
    /// Responsibilities:
    ///   • Prevent unhandled exceptions from propagating as 500 HTML pages.
    ///   • Produce a consistent JSON error envelope on every error path.
    ///   • Avoid leaking internal exception details (stack traces, SQL messages)
    ///     to clients in production — show full detail only in Development.
    ///
    /// Placement: registered as the FIRST middleware after UseForwardedHeaders()
    /// so it wraps the entire pipeline, including auth and routing errors.
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next   = next;
            _logger = logger;
            _env    = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception on {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Do not overwrite a response that has already started streaming.
            if (context.Response.HasStarted)
                return Task.CompletedTask;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = StatusCodes.Status500InternalServerError;

            // In Development: expose exception details so developers can diagnose quickly.
            // In Production:  return a generic message so internal details are never leaked.
            var response = _env.IsDevelopment()
                ? new
                {
                    message   = exception.Message,
                    detail    = exception.StackTrace,
                    traceId   = context.TraceIdentifier,
                    timestamp = DateTime.UtcNow
                }
                : (object)new
                {
                    message   = "An unexpected error occurred. Please try again later.",
                    traceId   = context.TraceIdentifier,
                    timestamp = DateTime.UtcNow
                };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
