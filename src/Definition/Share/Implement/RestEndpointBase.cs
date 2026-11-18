using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;

namespace Share.Implement;

/// <summary>Base type for classes that map Minimal API endpoint groups.</summary>
public abstract class RestEndpointBase
{
    /// <summary>Internal extension used to pass localization format arguments to the writer.</summary>
    public const string LocalizationArgumentsExtensionName = "perigon.localizationArguments";

    /// <summary>A JSON success value or a Minimal API result.</summary>
    public sealed class ApiResult<T> : IResult, IEndpointMetadataProvider
    {
        private readonly IResult _result;
        private ApiResult(IResult result) => _result = result;

        public static implicit operator ApiResult<T>(T value) => new(TypedResults.Ok(value));
        public static implicit operator ApiResult<T>(Ok<T> result) => new(result);
        public static implicit operator ApiResult<T>(Created<T> result) => new(result);
        public static implicit operator ApiResult<T>(Accepted<T> result) => new(result);
        public static implicit operator ApiResult<T>(Ok result) => new(result);
        public static implicit operator ApiResult<T>(Created result) => new(result);
        public static implicit operator ApiResult<T>(Accepted result) => new(result);
        public static implicit operator ApiResult<T>(NoContent result) => new(result);
        public static implicit operator ApiResult<T>(BadRequest result) => new(result);
        public static implicit operator ApiResult<T>(NotFound result) => new(result);
        public static implicit operator ApiResult<T>(Conflict result) => new(result);
        public static implicit operator ApiResult<T>(UnauthorizedHttpResult result) => new(result);
        public static implicit operator ApiResult<T>(ForbidHttpResult result) => new(result);
        public static implicit operator ApiResult<T>(ProblemHttpResult result) => new(result);

        /// <summary>Wrap any other result; declare its response metadata on the endpoint.</summary>
        public static ApiResult<T> FromResult(IResult result) => new(result ?? throw new ArgumentNullException(nameof(result)));

        public Task ExecuteAsync(HttpContext httpContext) => _result.ExecuteAsync(httpContext);

        static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
        {
            PopulateMetadata<Ok<T>>(method, builder);
            builder.Metadata.Add(new ProducesResponseTypeMetadata(
                StatusCodes.Status400BadRequest, typeof(ErrorResult), ["application/problem+json"]));
        }

        private static void PopulateMetadata<TResult>(MethodInfo method, EndpointBuilder builder)
            where TResult : IEndpointMetadataProvider => TResult.PopulateMetadata(method, builder);
    }

    protected static RouteGroupBuilder MapGroup(
        IEndpointRouteBuilder endpoints,
        string pattern,
        string tag,
        string? authorizationPolicy = null)
    {
        var group = endpoints.MapGroup(pattern).WithTags(tag);
        return string.IsNullOrWhiteSpace(authorizationPolicy)
            ? group
            : group.RequireAuthorization(authorizationPolicy);
    }

    protected static ProblemHttpResult BadRequest(string? detail = null, params object[] arguments) =>
        Problem(detail, StatusCodes.Status400BadRequest, arguments);

    protected static ProblemHttpResult NotFound(string? detail = null, params object[] arguments) =>
        Problem(detail, StatusCodes.Status404NotFound, arguments);

    protected static ProblemHttpResult Forbid(string? detail = null, params object[] arguments) =>
        Problem(detail, StatusCodes.Status403Forbidden, arguments);

    protected static ProblemHttpResult Conflict(string? detail = null, params object[] arguments) =>
        Problem(detail, StatusCodes.Status409Conflict, arguments);

    protected static ProblemHttpResult Problem(
        string? detail = null,
        int statusCode = StatusCodes.Status500InternalServerError,
        params object[] arguments)
    {
        var extensions = arguments.Length == 0 ? null : new Dictionary<string, object?>
        {
            [LocalizationArgumentsExtensionName] = arguments,
        };
        return TypedResults.Problem(detail: detail, statusCode: statusCode, extensions: extensions);
    }
}
