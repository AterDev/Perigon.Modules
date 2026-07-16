using ApiTest.Data;
using Entity.ResourceMod;
using ResourceMod.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ResourceEntity = Entity.ResourceMod.Resource;

namespace ApiTest;

public class ResourceModApiTests
{
    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task ConfigurationApis_ShouldMaintainNavigationData(TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        string suffix = Guid.NewGuid().ToString("N");

        ResEnvironment environment = await PostAsync<ResEnvironment>(
            client,
            "/api/ResourceConfiguration/environments",
            new ResEnvironmentInput { Name = $"Environment-{suffix}", Color = "#123456" },
            HttpStatusCode.OK);
        ResCategory category = await PostAsync<ResCategory>(
            client,
            "/api/ResourceConfiguration/categories",
            new ResCategoryInput
            {
                Name = $"Category-{suffix}",
                CatalogCode = $"catalog-{suffix}",
                Color = "#654321"
            },
            HttpStatusCode.OK);
        ResGroup group = await PostAsync<ResGroup>(
            client,
            "/api/ResourceConfiguration/groups",
            new ResGroupInput
            {
                Name = $"Group-{suffix}",
                CategoryId = category.Id,
                Color = "#abcdef"
            },
            HttpStatusCode.OK);
        ResTag tag = await PostAsync<ResTag>(
            client,
            "/api/ResourceConfiguration/tags",
            new ResTagInput { Name = $"Tag-{suffix}", Color = "#fedcba" },
            HttpStatusCode.OK);

        ResDefinition definition = await PostAsync<ResDefinition>(
            client,
            "/api/ResourceConfiguration/definitions",
            DefinitionInput(suffix),
            HttpStatusCode.OK);

        HttpResponseMessage groupListResponse = await client.GetAsync(
            $"/api/ResourceConfiguration/groups?categoryId={category.Id}");
        await Assert.That(groupListResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        List<ResGroup>? groups = await groupListResponse.Content.ReadFromJsonAsync<List<ResGroup>>();
        await Assert.That(groups).IsNotNull();
        await Assert.That(groups!.Any(item => item.Id == group.Id)).IsTrue();
        await Assert.That(groups.All(item => item.CategoryId == category.Id)).IsTrue();

        HttpResponseMessage definitionListResponse = await client.GetAsync(
            "/api/ResourceConfiguration/definitions");
        await Assert.That(definitionListResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        List<ResDefinition>? definitions = await definitionListResponse.Content
            .ReadFromJsonAsync<List<ResDefinition>>();
        await Assert.That(definitions).IsNotNull();
        ResDefinition listed = definitions!.Single(item => item.Id == definition.Id);
        await Assert.That(listed.Properties.OrderBy(item => item.Sort).Select(item => item.Name))
            .IsEquivalentTo(["Address", "Enabled", "Label", "Port", "Uri", "When"]);

        environment = await PutAsync<ResEnvironment>(
            client,
            $"/api/ResourceConfiguration/environments/{environment.Id}",
            new ResEnvironmentInput { Name = $"Environment-Updated-{suffix}", Color = "#111111" },
            HttpStatusCode.OK);
        category = await PutAsync<ResCategory>(
            client,
            $"/api/ResourceConfiguration/categories/{category.Id}",
            new ResCategoryInput
            {
                Name = $"Category-Updated-{suffix}",
                CatalogCode = $"catalog-updated-{suffix}",
                Color = "#222222"
            },
            HttpStatusCode.OK);
        group = await PutAsync<ResGroup>(
            client,
            $"/api/ResourceConfiguration/groups/{group.Id}",
            new ResGroupInput
            {
                Name = $"Group-Updated-{suffix}",
                CategoryId = category.Id,
                Color = "#333333"
            },
            HttpStatusCode.OK);
        tag = await PutAsync<ResTag>(
            client,
            $"/api/ResourceConfiguration/tags/{tag.Id}",
            new ResTagInput { Name = $"Tag-Updated-{suffix}", Color = "#444444" },
            HttpStatusCode.OK);

        await Assert.That(environment.Name).IsEqualTo($"Environment-Updated-{suffix}");
        await Assert.That(category.CatalogCode).IsEqualTo($"catalog-updated-{suffix}");
        await Assert.That(group.CategoryId).IsEqualTo(category.Id);
        await Assert.That(tag.Name).IsEqualTo($"Tag-Updated-{suffix}");

        await DeleteAsync(client, $"/api/ResourceConfiguration/tags/{tag.Id}", HttpStatusCode.OK);
        await DeleteAsync(client, $"/api/ResourceConfiguration/groups/{group.Id}", HttpStatusCode.OK);
        await DeleteAsync(client, $"/api/ResourceConfiguration/environments/{environment.Id}", HttpStatusCode.OK);
        await DeleteAsync(client, $"/api/ResourceConfiguration/categories/{category.Id}", HttpStatusCode.OK);
        await DeleteAsync(client, $"/api/ResourceConfiguration/definitions/{definition.Id}", HttpStatusCode.OK);
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task DefinitionApi_ShouldUpdateExistingAndAddNewProperty(TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        string suffix = Guid.NewGuid().ToString("N");
        ResDefinition definition = await PostAsync<ResDefinition>(
            client,
            "/api/ResourceConfiguration/definitions",
            DefinitionInput(suffix),
            HttpStatusCode.OK);

        List<ResDefinitionPropertyInput> properties = definition.Properties
            .Select(property => new ResDefinitionPropertyInput
            {
                Id = property.Id,
                Name = property.Name,
                ValueType = property.ValueType,
                IsRequired = property.IsRequired,
                MaxLength = property.MaxLength,
                Sort = property.Sort
            })
            .ToList();
        properties.Add(new ResDefinitionPropertyInput
        {
            Name = "Host",
            ValueType = ResValueType.String,
            IsRequired = false,
            MaxLength = 200,
            Sort = properties.Count
        });

        ResDefinition updated = await PutAsync<ResDefinition>(
            client,
            $"/api/ResourceConfiguration/definitions/{definition.Id}",
            new ResDefinitionInput
            {
                Name = definition.Name,
                Icon = definition.Icon,
                Properties = properties
            },
            HttpStatusCode.OK);

        await Assert.That(updated.Properties.Any(property => property.Name == "Host")).IsTrue();
        await DeleteAsync(client, $"/api/ResourceConfiguration/definitions/{definition.Id}", HttpStatusCode.OK);
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task ResourceApis_ShouldValidateNormalizeFilterAndSoftDelete(TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        ResourceFixture fixture = await CreateFixtureAsync(client);

        ResourceEntity resource = await PostAsync<ResourceEntity>(
            client,
            "/api/Resource",
            new ResourceInput
            {
                Name = fixture.Name,
                Description = "initial",
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                GroupId = fixture.Group.Id,
                DefinitionId = fixture.Definition.Id,
                TagNames = [fixture.Tag.Name, fixture.Tag.Name, "Other"],
                Values = fixture.Values
            },
            HttpStatusCode.Created);

        HttpResponseMessage detailResponse = await client.GetAsync($"/api/Resource/{resource.Id}");
        await Assert.That(detailResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        ResourceDetailDto? detail = await detailResponse.Content.ReadFromJsonAsync<ResourceDetailDto>();
        await Assert.That(detail).IsNotNull();
        await Assert.That(detail!.TagNames).IsEquivalentTo([fixture.Tag.Name, "Other"]);
        await Assert.That(detail.EnvironmentName).IsEqualTo(fixture.Environment.Name);
        await Assert.That(detail.CategoryName).IsEqualTo(fixture.Category.Name);
        await Assert.That(detail.GroupName).IsEqualTo(fixture.Group.Name);
        await Assert.That(detail.Values.Select(value => value.Value))
            .IsEquivalentTo(["192.168.0.1", "2026-07-15", "true", "80", "https://example.com/", "server"]);

        HttpResponseMessage listResponse = await client.GetAsync(
            $"/api/Resource/list?name={Uri.EscapeDataString(fixture.Name)}&tagName=Other");
        await Assert.That(listResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        JsonDocument list = await listResponse.Content.ReadFromJsonAsync<JsonDocument>()
            ?? throw new InvalidOperationException("Resource list response was empty.");
        await Assert.That(list.RootElement.GetProperty("count").GetInt32()).IsEqualTo(1);
        await Assert.That(list.RootElement.GetProperty("data").EnumerateArray().Single()
            .GetProperty("id").GetGuid()).IsEqualTo(resource.Id);

        HttpResponseMessage updateResponse = await PatchAsync(
            client,
            $"/api/Resource/{resource.Id}",
            new ResourceInput
            {
                Name = $"{fixture.Name}-Updated",
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                DefinitionId = fixture.Definition.Id,
                TagNames = ["Updated"],
                Values = fixture.Values
            });
        if (updateResponse.StatusCode != HttpStatusCode.OK)
        {
            string error = await updateResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Resource update returned {(int)updateResponse.StatusCode}: {error}");
        }
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        ResourceDetailDto? updated = await GetAsync<ResourceDetailDto>(
            client,
            $"/api/Resource/{resource.Id}",
            HttpStatusCode.OK);
        await Assert.That(updated!.Name).IsEqualTo($"{fixture.Name}-Updated");
        await Assert.That(updated.GroupId).IsNull();
        await Assert.That(updated.TagNames).IsEquivalentTo(["Updated"]);

        await DeleteAsync(client, $"/api/Resource/{resource.Id}", HttpStatusCode.OK);
        HttpResponseMessage deletedDetail = await client.GetAsync($"/api/Resource/{resource.Id}");
        await Assert.That(deletedDetail.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task ResourceAndDefinitionApis_ShouldRejectInvalidValuesAndReferencedDeletes(
        TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        ResourceFixture fixture = await CreateFixtureAsync(client);

        HttpResponseMessage missingRequired = await client.PostAsJsonAsync(
            "/api/Resource",
            new ResourceInput
            {
                Name = $"Invalid-{fixture.Name}",
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                DefinitionId = fixture.Definition.Id,
                Values = []
            });
        await Assert.That(missingRequired.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        ResourceEntity resource = await PostAsync<ResourceEntity>(
            client,
            "/api/Resource",
            new ResourceInput
            {
                Name = fixture.Name,
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                GroupId = fixture.Group.Id,
                DefinitionId = fixture.Definition.Id,
                Values = fixture.Values
            },
            HttpStatusCode.Created);

        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/environments/{fixture.Environment.Id}",
            HttpStatusCode.Conflict);
        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/categories/{fixture.Category.Id}",
            HttpStatusCode.Conflict);
        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/definitions/{fixture.Definition.Id}",
            HttpStatusCode.Conflict);
        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/groups/{fixture.Group.Id}",
            HttpStatusCode.Conflict);

        ResDefinition updatedDefinition = await PutAsync<ResDefinition>(
            client,
            $"/api/ResourceConfiguration/definitions/{fixture.Definition.Id}",
            new ResDefinitionInput
            {
                Name = fixture.Definition.Name,
                Properties = fixture.Definition.Properties
                    .Select(property => new ResDefinitionPropertyInput
                    {
                        Id = property.Id,
                        Name = property.Name == "Address" ? "Host" : property.Name,
                        ValueType = property.ValueType,
                        IsRequired = property.IsRequired,
                        MaxLength = property.MaxLength,
                        Sort = property.Sort
                    })
                    .ToList()
            },
            HttpStatusCode.OK);
        await Assert.That(updatedDefinition.Properties.Single(p => p.Id == fixture.AddressProperty.Id).Name)
            .IsEqualTo("Host");

        await DeleteAsync(client, $"/api/Resource/{resource.Id}", HttpStatusCode.OK);
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task PermissionApi_ShouldReplaceRolesForEnvironmentAndCategory(TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        ResourceFixture fixture = await CreateFixtureAsync(client);
        JsonDocument roleList = await GetAsync<JsonDocument>(client, "/api/SystemRole?pageSize=20", HttpStatusCode.OK);
        Guid roleId = roleList.RootElement.GetProperty("data").EnumerateArray().First()
            .GetProperty("id").GetGuid();

        HttpResponseMessage setResponse = await client.PutAsJsonAsync(
            "/api/ResourceConfiguration/permissions",
            new ResPermissionInput
            {
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                RoleIds = [roleId, roleId]
            });
        await Assert.That(setResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        List<ResPermission>? permissions = await GetAsync<List<ResPermission>>(
            client,
            $"/api/ResourceConfiguration/permissions?environmentId={fixture.Environment.Id}&categoryId={fixture.Category.Id}",
            HttpStatusCode.OK);
        await Assert.That(permissions).IsNotNull();
        await Assert.That(permissions!.Count).IsEqualTo(1);
        await Assert.That(permissions[0].RoleId).IsEqualTo(roleId);

        HttpResponseMessage clearPermissions = await client.PutAsJsonAsync(
            "/api/ResourceConfiguration/permissions",
            new ResPermissionInput
            {
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                RoleIds = []
            });
        await Assert.That(clearPermissions.StatusCode).IsEqualTo(HttpStatusCode.OK);

        await DeleteAsync(client, $"/api/ResourceConfiguration/groups/{fixture.Group.Id}", HttpStatusCode.OK);
        await DeleteAsync(client, $"/api/ResourceConfiguration/categories/{fixture.Category.Id}", HttpStatusCode.OK);
        await DeleteAsync(client, $"/api/ResourceConfiguration/environments/{fixture.Environment.Id}", HttpStatusCode.OK);
        await DeleteAsync(client, $"/api/ResourceConfiguration/definitions/{fixture.Definition.Id}", HttpStatusCode.OK);
        await DeleteAsync(client, $"/api/ResourceConfiguration/tags/{fixture.Tag.Id}", HttpStatusCode.OK);
    }

    private static ResDefinitionInput DefinitionInput(string suffix)
    {
        return new ResDefinitionInput
        {
            Name = $"Definition-{suffix}",
            Properties =
            [
                new() { Name = "Address", ValueType = ResValueType.IPAddress, IsRequired = true, MaxLength = 40, Sort = 2 },
                new() { Name = "Enabled", ValueType = ResValueType.Boolean, IsRequired = true, MaxLength = 10, Sort = 3 },
                new() { Name = "Label", ValueType = ResValueType.String, IsRequired = true, MaxLength = 60, Sort = 6 },
                new() { Name = "Port", ValueType = ResValueType.Number, IsRequired = true, MaxLength = 10, Sort = 1 },
                new() { Name = "Uri", ValueType = ResValueType.Uri, IsRequired = true, MaxLength = 200, Sort = 5 },
                new() { Name = "When", ValueType = ResValueType.Date, IsRequired = true, MaxLength = 20, Sort = 4 }
            ]
        };
    }

    private static async Task<ResourceFixture> CreateFixtureAsync(HttpClient client)
    {
        string suffix = Guid.NewGuid().ToString("N");
        ResEnvironment environment = await PostAsync<ResEnvironment>(
            client,
            "/api/ResourceConfiguration/environments",
            new ResEnvironmentInput { Name = $"Environment-{suffix}", Color = "#123456" },
            HttpStatusCode.OK);
        ResCategory category = await PostAsync<ResCategory>(
            client,
            "/api/ResourceConfiguration/categories",
            new ResCategoryInput
            {
                Name = $"Category-{suffix}",
                CatalogCode = $"catalog-{suffix}",
                Color = "#654321"
            },
            HttpStatusCode.OK);
        ResGroup group = await PostAsync<ResGroup>(
            client,
            "/api/ResourceConfiguration/groups",
            new ResGroupInput { Name = $"Group-{suffix}", CategoryId = category.Id, Color = "#abcdef" },
            HttpStatusCode.OK);
        ResTag tag = await PostAsync<ResTag>(
            client,
            "/api/ResourceConfiguration/tags",
            new ResTagInput { Name = $"Tag-{suffix}", Color = "#fedcba" },
            HttpStatusCode.OK);
        ResDefinition definition = await PostAsync<ResDefinition>(
            client,
            "/api/ResourceConfiguration/definitions",
            DefinitionInput(suffix),
            HttpStatusCode.OK);

        ResDefinitionProperty address = definition.Properties.Single(property => property.Name == "Address");
        ResDefinitionProperty enabled = definition.Properties.Single(property => property.Name == "Enabled");
        ResDefinitionProperty label = definition.Properties.Single(property => property.Name == "Label");
        ResDefinitionProperty port = definition.Properties.Single(property => property.Name == "Port");
        ResDefinitionProperty uri = definition.Properties.Single(property => property.Name == "Uri");
        ResDefinitionProperty when = definition.Properties.Single(property => property.Name == "When");

        return new ResourceFixture(
            environment,
            category,
            group,
            tag,
            definition,
            $"Resource-{suffix}",
            address,
            [
                new ResourceValueInput { DefinitionPropertyId = address.Id, Value = "192.168.000.001" },
                new ResourceValueInput { DefinitionPropertyId = enabled.Id, Value = "TRUE" },
                new ResourceValueInput { DefinitionPropertyId = label.Id, Value = "server" },
                new ResourceValueInput { DefinitionPropertyId = port.Id, Value = "80" },
                new ResourceValueInput { DefinitionPropertyId = uri.Id, Value = "https://example.com" },
                new ResourceValueInput { DefinitionPropertyId = when.Id, Value = "2026-07-15" }
            ]);
    }

    private static async Task<T> GetAsync<T>(HttpClient client, string path, HttpStatusCode status)
    {
        HttpResponseMessage response = await client.GetAsync(path);
        await Assert.That(response.StatusCode).IsEqualTo(status);
        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"Response for {path} was empty.");
    }

    private static async Task<T> PostAsync<T>(
        HttpClient client,
        string path,
        object body,
        HttpStatusCode status)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(path, body);
        await Assert.That(response.StatusCode).IsEqualTo(status);
        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"Response for {path} was empty.");
    }

    private static async Task<T> PutAsync<T>(
        HttpClient client,
        string path,
        object body,
        HttpStatusCode status)
    {
        HttpResponseMessage response = await client.PutAsJsonAsync(path, body);
        if (response.StatusCode != status)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"PUT {path} returned {(int)response.StatusCode}: {error}");
        }
        await Assert.That(response.StatusCode).IsEqualTo(status);
        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"Response for {path} was empty.");
    }

    private static async Task<HttpResponseMessage> PatchAsync(HttpClient client, string path, object body)
    {
        return await client.PatchAsJsonAsync(path, body);
    }

    private static async Task DeleteAsync(HttpClient client, string path, HttpStatusCode status)
    {
        HttpResponseMessage response = await client.DeleteAsync(path);
        if (response.StatusCode != status)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"DELETE {path} returned {(int)response.StatusCode}: {error}");
        }
        await Assert.That(response.StatusCode).IsEqualTo(status);
    }

    private sealed record ResourceFixture(
        ResEnvironment Environment,
        ResCategory Category,
        ResGroup Group,
        ResTag Tag,
        ResDefinition Definition,
        string Name,
        ResDefinitionProperty AddressProperty,
        List<ResourceValueInput> Values);
}
