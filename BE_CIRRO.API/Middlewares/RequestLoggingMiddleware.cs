using System.Diagnostics;

namespace BE_CIRRO.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch (TaskCanceledException)
        {
            // Request bị hủy (client đóng tab, reload, timeout)
            _logger.LogWarning("Request was canceled: {Method} {Path}", context.Request.Method, context.Request.Path);
            return;
        }
        finally
        {
            stopwatch.Stop();

            var logLevel = context.Response.StatusCode >= 500
                ? LogLevel.Error
                : context.Response.StatusCode >= 400
                    ? LogLevel.Warning
                    : LogLevel.Information;

            _logger.Log(
                logLevel,
                "HTTP {Method} {Path} responded {StatusCode} in {Elapsed:0.0000} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds
            );
        }
    }
}
