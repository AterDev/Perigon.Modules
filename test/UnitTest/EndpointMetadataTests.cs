using ApiService.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Perigon.AspNetCore.Constants;

namespace UnitTest;

public sealed class EndpointMetadataTests
{
    [Test]
    public async Task NotificationEndpoints_ShouldSeparateUserAndAdminRoutes()
    {
        using WebApplication app = BuildApplication();
        NotificationEndpoints.MapEndpoints(app);

        RouteEndpoint mine = Find(app, "/api/Notification/mine", "POST");
        RouteEndpoint admin = Find(app, "/api/Notification", "POST");

        await Assert.That(Policies(mine)).Contains(WebConst.User);
        await Assert.That(Policies(mine)).DoesNotContain(WebConst.AdminUser);
        await Assert.That(Policies(admin)).Contains(WebConst.User);
        await Assert.That(Policies(admin)).Contains(WebConst.AdminUser);
    }

    [Test]
    public async Task NotificationWebhook_ShouldAllowAnonymous()
    {
        using WebApplication app = BuildApplication();
        NotificationWebhookEndpoints.MapEndpoints(app);

        RouteEndpoint webhook = Find(app, "/api/webhooks/{sourceId:guid}", "POST");

        await Assert.That(webhook.Metadata.GetMetadata<IAllowAnonymous>()).IsNotNull();
        await Assert.That(webhook.Metadata.GetMetadata<IAuthorizeData>()).IsNull();
    }

    [Test]
    public async Task AuditEndpoints_ShouldRequireAdminOrAuthenticatedUserAsDesigned()
    {
        using WebApplication app = BuildApplication();
        AuditFlowEndpoints.MapEndpoints(app);
        AdminAuditRecordEndpoints.MapEndpoints(app);
        AuditRecordEndpoints.MapEndpoints(app);
        AuditTaskEndpoints.MapEndpoints(app);

        RouteEndpoint flowAdmin = Find(app, "/api/admin/audit-flows", "GET");
        RouteEndpoint allRecordsAdmin = Find(app, "/api/admin/audit-records", "GET");
        RouteEndpoint submit = Find(app, "/api/audit-records", "POST");
        RouteEndpoint approve = Find(app, "/api/audit-tasks/{id:guid}/approve", "POST");

        await Assert.That(Policies(flowAdmin)).Contains(WebConst.AdminUser);
        await Assert.That(Policies(allRecordsAdmin)).Contains(WebConst.AdminUser);
        await Assert.That(submit.Metadata.GetMetadata<IAuthorizeData>()).IsNotNull();
        await Assert.That(Policies(submit)).DoesNotContain(WebConst.AdminUser);
        await Assert.That(approve.Metadata.GetMetadata<IAuthorizeData>()).IsNotNull();
        await Assert.That(Policies(approve)).DoesNotContain(WebConst.AdminUser);
    }

    private static WebApplication BuildApplication() =>
        WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(EndpointMetadataTests).Assembly.GetName().Name,
            EnvironmentName = "UnitTest",
        }).Build();

    private static RouteEndpoint Find(WebApplication app, string route, string method)
    {
        RouteEndpoint[] endpoints = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();
        RouteEndpoint? match = endpoints.SingleOrDefault(endpoint =>
            Normalize(endpoint.RoutePattern.RawText) == Normalize(route) &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(method) == true);
        return match ?? throw new InvalidOperationException(
            $"Endpoint not found: {method} {route}. Actual: {string.Join(", ", endpoints.Select(endpoint => $"{string.Join('|', endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? [])} {endpoint.RoutePattern.RawText}"))}");
    }

    private static string Normalize(string? route) => route?.Trim('/') ?? string.Empty;

    private static IReadOnlyList<string> Policies(RouteEndpoint endpoint) =>
        endpoint.Metadata
            .GetOrderedMetadata<IAuthorizeData>()
            .Select(metadata => metadata.Policy ?? string.Empty)
            .ToArray();
}
