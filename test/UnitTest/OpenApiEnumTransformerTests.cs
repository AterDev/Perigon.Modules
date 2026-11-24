using ApiService.OpenApi;
using Entity.SystemMod;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using System.Net;
using System.Text.Json.Nodes;

namespace UnitTest;

public sealed class OpenApiEnumTransformerTests
{
    [Test]
    public async Task OpenApiDocument_DescribesEnumMembersUsingAnnotatedEnumerations()
    {
        await using WebApplication app = BuildApplication();
        await app.StartAsync();

        try
        {
            string address = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using HttpClient client = new()
            {
                BaseAddress = new Uri(address)
            };
            using HttpResponseMessage response = await client.GetAsync("/openapi/v1.json");

            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            JsonNode document = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
            await Assert.That(document["openapi"]!.GetValue<string>().StartsWith("3.1.", StringComparison.Ordinal)).IsTrue();
            JsonNode schema = document["components"]!["schemas"]!["Sex"]!;
            await Assert.That(schema["x-enumData"]).IsNull();
            JsonArray members = schema["oneOf"]!.AsArray();
            await Assert.That(members.Count).IsEqualTo(3);
            await Assert.That(members.Any(member => member!["const"]!.GetValue<int>() == 0
                && member["title"]!.GetValue<string>() == "Male"
                && member["description"]!.GetValue<string>() == "男性")).IsTrue();
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static WebApplication BuildApplication()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(OpenApiEnumTransformerTests).Assembly.GetName().Name,
            EnvironmentName = "UnitTest",
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services.AddOpenApi(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
            options.AddSchemaTransformer<EnumSchemaTransformer>();
        });

        WebApplication app = builder.Build();
        app.MapGet("/unit/openapi-enum", static () => new OpenApiEnumProbe
        {
            Sex = Sex.Male,
        });
        app.MapOpenApi();
        return app;
    }

    public sealed class OpenApiEnumProbe
    {
        public Sex Sex
        {
            get; set;
        }
    }
}
