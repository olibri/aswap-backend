//namespace Aswap_back.Middleware;

//public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
//{
//    public async Task InvokeAsync(HttpContext context)
//    {
//        try
//        {
//            await next(context);
//        }

//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Unhandled exception occurred.");
//            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//            await context.Response.WriteAsync("Internal server error");
//        }
//    }
//}