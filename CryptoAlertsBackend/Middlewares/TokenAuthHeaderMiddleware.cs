namespace CryptoAlertsBackend.Middlewares
{
    public class TokenAuthHeaderMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            string headerName = Environment.GetEnvironmentVariable("AUTH_TOKEN_HEADER_NAME");
            string headerTokenValueExpected = Environment.GetEnvironmentVariable("AUTH_TOKEN_HEADER_VALUE");

            if (string.IsNullOrEmpty(headerName) || string.IsNullOrEmpty(headerTokenValueExpected))
            {
                Console.WriteLine("Environment variables for header validation are not set.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Server configuration error.");
                return;
            }

            if (!context.Request.Headers.TryGetValue(headerName, out var token))
            {
                Console.WriteLine($"{context.Request.Host} did not use proper headers.");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing required authentication header.");
                return;
            }

            if (token != headerTokenValueExpected)
            {
                Console.WriteLine($"{context.Request.Host} used wrong token.");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Invalid authentication token.");
                return;
            }

            // If validation succeeds, continue to the next middleware
            await next(context);
        }
    }
}
