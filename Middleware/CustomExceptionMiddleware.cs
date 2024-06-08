namespace TaskManagementSystem.Middleware
{
    // Middleware tùy chỉnh để xử lý các ngoại lệ
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        // Constructor nhận RequestDelegate và lưu trữ để sử dụng sau
        public CustomExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Phương thức InvokeAsync là điểm vào của middleware
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Tiếp tục chuỗi middleware
                await _next(context);
            }
            catch (UnauthorizedAccessException)
            {
                // Xử lý ngoại lệ UnauthorizedAccessException và chuyển hướng đến trang AccessDenied
                context.Response.Redirect("/Account/AccessDenied");
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ khác nếu cần thiết
                throw;
            }
        }
    }
}