public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;

    public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // שמירת הזרם המקורי של ה-response
        var originalBodyStream = context.Response.Body;

        // יצירת זרם חדש כדי לקרוא את ה-response
        using (var responseBody = new MemoryStream())
        {
            // הגדרת ה-response Body לזרם החדש
            context.Response.Body = responseBody;

            try
            {
                // המשך טיפול בבקשה
                await _next(context);

                // קריאה ל-body של ה-response
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseBodyContent = await new StreamReader(context.Response.Body).ReadToEndAsync();

                // בדיקת קוד הסטטוס של ה-response
                if (context.Response.StatusCode >= 400)
                {
                    // רישום השגיאה בלוג
                    _logger.LogError($"Response Status Code: {context.Response.StatusCode}, Response Body: {responseBodyContent}");
                }

                // החזרת הזרם למקום המקורי
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                // טיפול בשגיאות במהלך הטיפול בבקשה
                _logger.LogError(ex, "An error occurred while processing the request.");
                throw;
            }
        }
    }
}
