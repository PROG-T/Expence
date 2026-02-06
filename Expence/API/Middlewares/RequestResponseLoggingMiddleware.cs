using System.Text;

namespace Expence.API.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        private static readonly HashSet<string> LoggableContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/json",
            "application/x-www-form-urlencoded",
            "text/plain"
        };

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1) Log request
            await LogRequestAsync(context);

            // 2) Swap response body with a buffer
            var originalBody = context.Response.Body;
            await using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await _next(context);

                // 3) Read response for logging
                buffer.Position = 0; // rewind before reading
                string responseText = "";
                if (buffer.Length > 0)
                {
                    using var reader = new StreamReader(buffer, leaveOpen: true);
                    responseText = await reader.ReadToEndAsync();
                }

                // 4) Log response meta/body
                LogResponseMeta(context);
                if (!string.IsNullOrEmpty(responseText))
                {
                    if (responseText.Length > 2000)
                        responseText = responseText[..2000] + "\n... (truncated)";

                    _logger.LogDebug("Response Body: {ResponseBody}", MaskSensitiveData(responseText));
                }

                // 5) Rewind and copy back to the original stream
                buffer.Position = 0; // rewind before copying back
                await buffer.CopyToAsync(originalBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in RequestResponseLoggingMiddleware for {Path}", context.Request.Path);
                throw;
            }
            finally
            {
                // 6) Restore original body no matter what
                context.Response.Body = originalBody;
            }
        }

        private async Task LogRequestAsync(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();

                string body = "";
                if (context.Request.ContentLength > 0 && IsLoggableContentType(context.Request.ContentType))
                {
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    if (body.Length > 2000)
                        body = body[..2000] + "\n... (truncated)";
                }

                var userId = context.User?.Identity?.Name ?? "Anonymous";
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                _logger.LogInformation(
                    "Request Details: Method: {Method}, Path: {Path}, QueryString: {QueryString}, ContentType: {ContentType}, ContentLength: {ContentLength}, UserId: {UserId}, ClientIP: {ClientIP}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString,
                    context.Request.ContentType,
                    context.Request.ContentLength ?? 0,
                    userId,
                    clientIp);

                if (!string.IsNullOrWhiteSpace(body))
                    _logger.LogDebug("Request Body: {RequestBody}", MaskSensitiveData(body));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log request for {Path}", context.Request.Path);
            }
        }

        private void LogResponseMeta(HttpContext context)
        {
            var userId = context.User?.Identity?.Name ?? "Anonymous";
            _logger.LogInformation(
                "Response Details: Path: {Path}, StatusCode: {StatusCode}, ContentType: {ContentType}, ContentLength: {ContentLength}, UserId: {UserId}",
                context.Request.Path,
                context.Response.StatusCode,
                context.Response.ContentType,
                context.Response.ContentLength ?? 0,
                userId);
        }

        private static bool IsLoggableContentType(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType)) return false;
            return LoggableContentTypes.Any(t => contentType.Contains(t, StringComparison.OrdinalIgnoreCase));
        }

        private static string MaskSensitiveData(string data)
        {
            if (string.IsNullOrEmpty(data)) return data;

            var patterns = new[]
            {
                ("\"password\"\\s*:\\s*\"[^\"]*\"", "\"password\":\"***\""),
                ("\"token\"\\s*:\\s*\"[^\"]*\"", "\"token\":\"***\""),
                ("\"refreshToken\"\\s*:\\s*\"[^\"]*\"", "\"refreshToken\":\"***\""),
                ("\"authorization\"\\s*:\\s*\"[^\"]*\"", "\"authorization\":\"***\""),
                ("\"ssn\"\\s*:\\s*\"[^\"]*\"", "\"ssn\":\"***\""),
                ("\"creditCard\"\\s*:\\s*\"[^\"]*\"", "\"creditCard\":\"***\"")
            };

            foreach (var (pattern, replacement) in patterns)
                data = System.Text.RegularExpressions.Regex.Replace(
                    data, pattern, replacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return data;
        }
    }
}