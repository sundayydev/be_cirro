namespace BE_CIRRO.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (TaskCanceledException)
        {
            // Trường hợp request bị hủy (thường khi client reload, đóng tab, hoặc timeout)
            _logger.LogWarning("Request was canceled by client: {Path}", context.Request.Path);
            // Không trả response vì kết nối đã bị hủy
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "Đã xảy ra lỗi hệ thống",
                    Error = ex.Message
                };

                await context.Response.WriteAsJsonAsync(response);
            }
            else
            {
                _logger.LogWarning("Response already started, cannot write error response.");
            }
        }
    }
}
