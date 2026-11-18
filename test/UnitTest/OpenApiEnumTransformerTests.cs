using ApiService.OpenApi;
using Entity.SystemMod;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace UnitTest;

public sealed class OpenApiEnumTransformerTests
{
    [Test]
    public async Task OpenApiDocument_ShouldContainLegacyEnumDataFromDescriptionAttributes()
    {
        await using WebApplication app = BuildApplication();
        await app.StartAsync();

        try
        {
            string address = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using HttpClient client = new() { BaseAddress = new Uri(address) };
            using HttpResponseMessage response = await client.GetAsync("/openapi/v1.json");

            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            using JsonDocument document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            bool found = ContainsEnumData(document.RootElement, "男性");
            await Assert.That(found).IsTrue();
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
        builder.Services.AddOpenApi(options => options.AddSchemaTransformer<EnumSchemaTransformer>());

        WebApplication app = builder.Build();
        app.MapGet("/unit/openapi-enum", static () => new OpenApiEnumProbe { Sex = Sex.Male });
        app.MapOpenApi();
        return app;
    }

    private static bool ContainsEnumData(JsonElement element, string expectedDescription)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("x-enumData", out JsonElement enumData)
                && enumData.ValueKind == JsonValueKind.Array
                && enumData.EnumerateArray().Any(item =>
                    item.TryGetProperty("description", out JsonElement description)
                    && description.GetString() == expectedDescription))
            {
                return true;
            }

            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (ContainsEnumData(property.Value, expectedDescription)) return true;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                if (ContainsEnumData(item, expectedDescription)) return true;
            }
        }

        return false;
    }

    public sealed class OpenApiEnumProbe
    {
        public Sex Sex { get; set; }
    }
}
