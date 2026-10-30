using Npgsql;
using Perigon.AspNetCore.Models;
using Share.Exceptions;

namespace ServiceDefaults.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, Localizer localizer)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;
            await ctx.Response.WriteAsJsonAsync(new ErrorResult(localizer.Get(Localizer.ConflictResource), ctx.TraceIdentifier));
        }
        catch (PostgresException ex)
        {
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(new ErrorResult(ex.Message, ctx.TraceIdentifier, "database error!"));
        }
        catch (BusinessException ex)
        {
            ctx.Response.StatusCode = ex.StatusCodes;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(localizer.Get(ex.LanguageKey), ctx.TraceIdentifier, status: ex.StatusCodes)
            );
        }
        catch (Exception ex)
        {
            // 非数据库类异常
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(new ErrorResult(ex.Message, ctx.TraceIdentifier));
        }
    }
}
