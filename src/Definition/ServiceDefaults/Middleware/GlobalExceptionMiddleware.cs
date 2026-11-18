using Npgsql;
using Microsoft.Extensions.Logging;
using Perigon.AspNetCore.Models;
using Share.Exceptions;

namespace ServiceDefaults.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, Localizer localizer, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (BadHttpRequestException ex) when (!ctx.Response.HasStarted)
        {
            ctx.Response.StatusCode = ex.StatusCode;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(ex.Message, ctx.TraceIdentifier, status: ex.StatusCode),
                MiddlewareJsonContext.Default.ErrorResult);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;
            await ctx.Response.WriteAsJsonAsync(new ErrorResult(localizer.Get(Localizer.ConflictResource), ctx.TraceIdentifier, status: StatusCodes.Status409Conflict), MiddlewareJsonContext.Default.ErrorResult);
        }
        catch (PostgresException ex)
        {
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(new ErrorResult(ex.Message, ctx.TraceIdentifier, "database error!"), MiddlewareJsonContext.Default.ErrorResult);
        }
        catch (BusinessException ex)
        {
            ctx.Response.StatusCode = ex.StatusCodes;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(localizer.Get(ex.LanguageKey), ctx.TraceIdentifier, status: ex.StatusCodes),
                MiddlewareJsonContext.Default.ErrorResult
            );
        }
        catch (Exception ex)
        {
            // 非数据库类异常
            logger.LogError(ex, "Unhandled request exception for {Path}", ctx.Request.Path);
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(new ErrorResult(ex.Message, ctx.TraceIdentifier), MiddlewareJsonContext.Default.ErrorResult);
        }
    }
}
