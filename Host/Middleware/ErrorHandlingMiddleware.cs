using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Errors;

namespace SoftwareCenter.Host.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IErrorHandler errorHandler)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceContext = new TraceContext(); // Create a new trace context for this error
                await errorHandler.HandleError(ex, traceContext, "An unhandled exception occurred in the Host.", isCritical: true);
                
                // Optionally, customize the response to the client
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
            }
        }
    }
}
