namespace TaskManagementSystem.Middleware
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException)
            {
                context.Response.Redirect("/Account/AccessDenied");
            }
            catch (Exception ex)
            {
                // Handle other exceptions if necessary
                throw;
            }
        }
    }
}