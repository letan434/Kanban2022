using System;
using KanbanApp.BackendServer.Helpers;
using Microsoft.AspNetCore.Builder;

namespace KanbanApp.BackendServer.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorWrapping(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorWrappingMiddleware>();
        }
    }
}
