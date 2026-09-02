using ApiTest.Data;
using Entity.ResourceMod;
using ResourceMod.Models.ResCategoryDtos;
using ResourceMod.Models.ResDefinitionDtos;
using ResourceMod.Models.ResDefinitionPropertyDtos;
using ResourceMod.Models.ResEnvironmentDtos;
using ResourceMod.Models.ResGroupDtos;
using ResourceMod.Models.ResPermissionDtos;
using ResourceMod.Models.ResTagDtos;
using ResourceMod.Models.ResourceDtos;
using ResourceMod.Models.PersonalResourceDtos;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ApiTest;

public class ResourceModApiTests
{
    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task ResourceModuleInitialization_ShouldCreateDefaultConfiguration(TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;

        List<ResEnvironment> environments = await GetAsync<List<ResEnvironment>>(
            client,
            "/api/ResourceConfiguration/environments",
            HttpStatusCode.OK);
        foreach ((string name, string color, string icon) in new[]
        {
            ("Development", "#4caf50", "code"),
            ("Test", "#2196f3", "science"),
            ("Production", "#f44336", "public")
        })
        {
            ResEnvironment environment = environments.Single(item => item.Name == name);
            await Assert.That(environment.Color).IsEqualTo(color);
            await Assert.That(environment.Icon).IsEqualTo(icon);
        }

        List<ResTag> tags = await GetAsync<List<ResTag>>(
            client,
            "/api/ResourceConfiguration/tags",
            HttpStatusCode.OK);
        foreach ((string name, string color, string icon) in new[]
        {
            ("Mac", "#9e9e9e", "desktop_mac"),
            ("Linux", "#ff9800", "terminal"),
            ("Windows", "#673ab7", "desktop_windows")
        })
        {
            ResTag tag = tags.Single(item => item.Name == name);
            await Assert.That(tag.Color).IsEqualTo(color);
            await Assert.That(tag.Icon).IsEqualTo(icon);
        }

        List<ResCategory> categories = await GetAsync<List<ResCategory>>(
            client,
            "/api/ResourceConfiguration/categories",
            HttpStatusCode.OK);
        await Assert.That(categories.Single(item => item.CatalogCode == "Default").Icon)
            .IsEqualTo("category");

        List<ResDefinitionProperty> properties = await GetAsync<List<ResDefinitionProperty>>(
            client,
            "/api/ResourceConfiguration/properties",
            HttpStatusCode.OK);
        Dictionary<string, ResValueType> expectedProperties = new()
        {
            ["名称"] = ResValueType.String,
            ["Url"] = ResValueType.Uri,
            ["描述"] = ResValueType.String,
            ["IP"] = ResValueType.IPAddress,
            ["Port"] = ResValueType.Number,
            ["用户名"] = ResValueType.String,
            ["密码"] = ResValueType.String,
            ["密钥"] = ResValueType.String,
            ["APIKey"] = ResValueType.String,
            ["Token"] = ResValueType.String,
            ["AppId"] = ResValueType.String,
            ["AppSecret"] = ResValueType.String,
            ["IconUrl"] = ResValueType.Uri
        };
        foreach ((string name, ResValueType valueType) in expectedProperties)
        {
            ResDefinitionProperty property = properties.Single(item => item.Name == name);
            await Assert.That(property.ValueType).IsEqualTo(valueType);
            await Assert.That(property.IsRequired).IsEqualTo(name == "名称");
        }

        List<ResDefinition> definitions = await GetAsync<List<ResDefinition>>(
            client,
            "/api/ResourceConfiguration/definitions",
            HttpStatusCode.OK);
        foreach ((string name, string icon, string[] propertyNames) in new[]
        {
            ("网站", "web", new[] { "名称", "Url", "IconUrl", "描述", "用户名", "密码" }),
            ("服务器", "dns", new[] { "名称", "IP", "Port", "用户名", "密码" }),
            ("数据库", "database", new[] { "名称", "IP", "Url", "Port", "用户名", "密码" })
        })
        {
            ResDefinition definition = definitions.Single(item => item.Name == name);
            await Assert.That(definition.Icon).IsEqualTo(icon);
            await Assert.That(definition.Properties.OrderBy(item => item.Sort).Select(item => item.Name))
                .IsEquivalentTo(propertyNames);
        }
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task PropertyApi_ShouldReusePropertiesAndProtectReferencedDeletes(TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        string suffix = Guid.NewGuid().ToString("N");
        ResDefinitionProperty property = await PostAsync<ResDefinitionProperty>(
            client,
            "/api/ResourceConfiguration/properties",
            new ResDefinitionPropertyAddDto
            {
                Name = $"Shared-{suffix}",
                ValueType = ResValueType.String,
                MaxLength = 100
            },
            HttpStatusCode.OK);

        ResDefinition first = await PostAsync<ResDefinition>(
            client,
            "/api/ResourceConfiguration/definitions",
            new ResDefinitionAddDto
            {
                Name = $"First-{suffix}",
                Properties =
                [
                    new ResDefinitionPropertyDto
                    {
                        Id = property.Id,
                        Name = property.Name,
                        ValueType = property.ValueType,
                        IsRequired = property.IsRequired,
                        MaxLength = property.MaxLength,
                        Sort = 0
                    }
                ]
            },
            HttpStatusCode.OK);
        ResDefinition second = await PostAsync<ResDefinition>(
            client,
            "/api/ResourceConfiguration/definitions",
            new ResDefinitionAddDto
            {
                Name = $"Second-{suffix}",
                Properties =
                [
                    new ResDefinitionPropertyDto
                    {
                        Id = property.Id,
                        Name = property.Name,
                        ValueType = property.ValueType,
                        IsRequired = property.IsRequired,
                        MaxLength = property.MaxLength,
                        Sort = 0
                    }
                ]
            },
            HttpStatusCode.OK);

        List<ResDefinitionProperty> listed = await GetAsync<List<ResDefinitionProperty>>(
            client,
            $"/api/ResourceConfiguration/properties?name=Shared-{suffix}",
            HttpStatusCode.OK);
        await Assert.That(listed.Select(item => item.Id)).IsEquivalentTo([property.Id]);

        property = await PutAsync<ResDefinitionProperty>(
            client,
            $"/api/ResourceConfiguration/properties/{property.Id}",
            new ResDefinitionPropertyUpdateDto
            {
                Name = $"Shared-Updated-{suffix}",
                ValueType = property.ValueType,
                IsRequired = property.IsRequired,
                MaxLength = property.MaxLength
            },
            HttpStatusCode.OK);
        await Assert.That(property.Name).IsEqualTo($"Shared-Updated-{suffix}");

        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/properties/{property.Id}",
            HttpStatusCode.Conflict);
        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/definitions/{first.Id}",
            HttpStatusCode.OK);
        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/properties/{property.Id}",
            HttpStatusCode.Conflict);
        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/definitions/{second.Id}",
            HttpStatusCode.OK);
        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/properties/{property.Id}",
            HttpStatusCode.OK);
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task PropertyAndDefinitionNames_ShouldRejectSpecialCharactersAndKeepCaseSensitiveUniqueness(
        TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        string suffix = Guid.NewGuid().ToString("N");

        HttpResponseMessage invalidProperty = await client.PostAsJsonAsync(
            "/api/ResourceConfiguration/properties",
            new ResDefinitionPropertyAddDto { Name = $"Invalid/{suffix}" });
        await Assert.That(invalidProperty.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        ResDefinitionProperty property = await PostAsync<ResDefinitionProperty>(
            client,
            "/api/ResourceConfiguration/properties",
            new ResDefinitionPropertyAddDto { Name = $"Case-{suffix}" },
            HttpStatusCode.OK);

        HttpResponseMessage duplicateProperty = await client.PostAsJsonAsync(
            "/api/ResourceConfiguration/properties",
            new ResDefinitionPropertyAddDto { Name = $"Case-{suffix}" });
        await Assert.That(duplicateProperty.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

        ResDefinitionProperty caseVariant = await PostAsync<ResDefinitionProperty>(
            client,
            "/api/ResourceConfiguration/properties",
            new ResDefinitionPropertyAddDto { Name = $"case-{suffix}" },
            HttpStatusCode.OK);
        await Assert.That(caseVariant.Id).IsNotEqualTo(property.Id);

        HttpResponseMessage invalidDefinition = await client.PostAsJsonAsync(
            "/api/ResourceConfiguration/definitions",
            new ResDefinitionAddDto { Name = $"Invalid/{suffix}" });
        await Assert.That(invalidDefinition.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/properties/{property.Id}",
            HttpStatusCode.OK);
        await DeleteAsync(
            client,
            $"/api/ResourceConfiguration/properties/{caseVariant.Id}",
            HttpStatusCode.OK);
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task ResourceValues_ShouldAllowSpecialCharacters(TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        ResourceFixture fixture = await CreateFixtureAsync(client);
        List<ResourceValueDto> values = fixture.Values
            .Select(value =>
            {
                Guid labelId = fixture.Definition.Properties.Single(property => property.Name == "Label").Id;
                return value.DefinitionPropertyId == labelId
                    ? new ResourceValueDto { DefinitionPropertyId = value.DefinitionPropertyId, Value = "server@host#1" }
                    : value;
            })
            .ToList();

        ResourceCreatedDto created = await PostAsync<ResourceCreatedDto>(
            client,
            "/api/Resource",
            new ResourceAddDto
            {
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                DefinitionId = fixture.Definition.Id,
                Values = values
            },
            HttpStatusCode.Created);

        ResourceDetailDto detail = await GetAsync<ResourceDetailDto>(
            client,
            $"/api/Resource/{created.Id}",
            HttpStatusCode.OK);
        await Assert.That(detail.Values.Any(value => value.Value == "server@host#1")).IsTrue();
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task ResourceValues_ShouldAllowEmptyOptionalValuesAndIdentifyInvalidProperty(
        TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        ResourceFixture fixture = await CreateFixtureAsync(client);
        string suffix = Guid.NewGuid().ToString("N");
        ResDefinition definition = await PostAsync<ResDefinition>(
            client,
            "/api/ResourceConfiguration/definitions",
            new ResDefinitionAddDto
            {
                Name = $"Optional-{suffix}",
                Properties =
                [
                    new()
                    {
                        Name = $"Name-{suffix}",
                        ValueType = ResValueType.String,
                        IsRequired = true,
                        MaxLength = 60,
                        Sort = 0
                    },
                    new()
                    {
                        Name = $"OptionalIp-{suffix}",
                        ValueType = ResValueType.IPAddress,
                        IsRequired = false,
                        MaxLength = 40,
                        Sort = 1
                    }
                ]
            },
            HttpStatusCode.OK);
        ResDefinitionProperty required = definition.Properties.Single(property => property.Name.StartsWith("Name-"));
        ResDefinitionProperty optional = definition.Properties.Single(property => property.Name.StartsWith("OptionalIp-"));

        HttpResponseMessage optionalEmpty = await client.PostAsJsonAsync(
            "/api/Resource",
            new ResourceAddDto
            {
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                DefinitionId = definition.Id,
                Values =
                [
                    new ResourceValueDto { DefinitionPropertyId = required.Id, Value = "server" },
                    new ResourceValueDto { DefinitionPropertyId = optional.Id, Value = string.Empty }
                ]
            });
        if (optionalEmpty.StatusCode != HttpStatusCode.Created)
        {
            string error = await optionalEmpty.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Optional empty resource returned {(int)optionalEmpty.StatusCode}: {error}");
        }
        ResourceCreatedDto created = await optionalEmpty.Content.ReadFromJsonAsync<ResourceCreatedDto>()
            ?? throw new InvalidOperationException("Created resource response was empty.");

        ResourceDetailDto detail = await GetAsync<ResourceDetailDto>(
            client,
            $"/api/Resource/{created.Id}",
            HttpStatusCode.OK);
        await Assert.That(detail.Values.Select(value => value.DefinitionPropertyId))
            .IsEquivalentTo([required.Id]);

        HttpResponseMessage invalid = await client.PostAsJsonAsync(
            "/api/Resource",
            new ResourceAddDto
            {
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                DefinitionId = definition.Id,
                Values =
                [
                    new ResourceValueDto { DefinitionPropertyId = required.Id, Value = "server" },
                    new ResourceValueDto { DefinitionPropertyId = optional.Id, Value = "not-an-ip" }
                ]
            });
        await Assert.That(invalid.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        JsonDocument problem = await invalid.Content.ReadFromJsonAsync<JsonDocument>()
            ?? throw new InvalidOperationException("Invalid resource response was empty.");
        await Assert.That(problem.RootElement.GetProperty("detail").GetString())
            .Contains(optional.Name);

        await DeleteAsync(client, $"/api/Resource/{created.Id}", HttpStatusCode.OK);
        await DeleteAsync(client, $"/api/ResourceConfiguration/definitions/{definition.Id}", HttpStatusCode.OK);
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task ConfigurationApis_ShouldMaintainNavigationData(TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        string suffix = Guid.NewGuid().ToString("N");

        ResEnvironment environment = await PostAsync<ResEnvironment>(
            client,
            "/api/ResourceConfiguration/environments",
            new ResEnvironmentAddDto { Name = $"Environment-{suffix}", Color = "#123456" },
            HttpStatusCode.OK);
        ResCategory category = await PostAsync<ResCategory>(
            client,
            "/api/ResourceConfiguration/categories",
            new ResCategoryAddDto
            {
                Name = $"Category-{suffix}",
                CatalogCode = $"catalog-{suffix}",
                Color = "#654321"
            },
            HttpStatusCode.OK);
        ResGroup group = await PostAsync<ResGroup>(
            client,
            "/api/ResourceConfiguration/groups",
            new ResGroupAddDto
            {
                Name = $"Group-{suffix}",
                CategoryId = category.Id,
                Color = "#abcdef"
            },
            HttpStatusCode.OK);
        ResTag tag = await PostAsync<ResTag>(
            client,
            "/api/ResourceConfiguration/tags",
            new ResTagAddDto { Name = $"Tag-{suffix}", Color = "#fedcba" },
            HttpStatusCode.OK);

        ResDefinition definition = await PostAsync<ResDefinition>(
            client,
            "/api/ResourceConfiguration/definitions",
            DefinitionInput(suffix),
            HttpStatusCode.OK);

        List<ResEnvironment> environments = await GetAsync<List<ResEnvironment>>(
            client,
            "/api/ResourceConfiguration/environments",
            HttpStatusCode.OK);
        await Assert.That(environments.Any(item => item.Id == environment.Id)).IsTrue();
        List<ResCategory> categories = await GetAsync<List<ResCategory>>(
            client,
            "/api/ResourceConfiguration/categories",
            HttpStatusCode.OK);
        await Assert.That(categories.Any(item => item.Id == category.Id)).IsTrue();
        List<ResTag> tags = await GetAsync<List<ResTag>>(
            client,
            "/api/ResourceConfiguration/tags",
            HttpStatusCode.OK);
        await Assert.That(tags.Any(item => item.Id == tag.Id)).IsTrue();

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
            .IsEquivalentTo(["Address", "Enabled", "Label", "TestPort", "Uri", "When"]);

        environment = await PutAsync<ResEnvironment>(
            client,
            $"/api/ResourceConfiguration/environments/{environment.Id}",
            new ResEnvironmentAddDto { Name = $"Environment-Updated-{suffix}", Color = "#111111" },
            HttpStatusCode.OK);
        category = await PutAsync<ResCategory>(
            client,
            $"/api/ResourceConfiguration/categories/{category.Id}",
            new ResCategoryAddDto
            {
                Name = $"Category-Updated-{suffix}",
                CatalogCode = $"catalog-updated-{suffix}",
                Color = "#222222"
            },
            HttpStatusCode.OK);
        group = await PutAsync<ResGroup>(
            client,
            $"/api/ResourceConfiguration/groups/{group.Id}",
            new ResGroupAddDto
            {
                Name = $"Group-Updated-{suffix}",
                CategoryId = category.Id,
                Color = "#333333"
            },
            HttpStatusCode.OK);
        tag = await PutAsync<ResTag>(
            client,
            $"/api/ResourceConfiguration/tags/{tag.Id}",
            new ResTagAddDto { Name = $"Tag-Updated-{suffix}", Color = "#444444" },
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

        List<ResDefinitionPropertyDto> properties = definition.Properties
            .Select(property => new ResDefinitionPropertyDto
            {
                Id = property.Id,
                Name = property.Name,
                ValueType = property.ValueType,
                IsRequired = property.IsRequired,
                MaxLength = property.MaxLength,
                Sort = property.Sort
            })
            .ToList();
        properties.Add(new ResDefinitionPropertyDto
        {
            Name = $"Host-{suffix}",
            ValueType = ResValueType.String,
            IsRequired = false,
            MaxLength = 200,
            Sort = properties.Count
        });

        ResDefinition updated = await PutAsync<ResDefinition>(
            client,
            $"/api/ResourceConfiguration/definitions/{definition.Id}",
            new ResDefinitionAddDto
            {
                Name = definition.Name,
                Icon = definition.Icon,
                Properties = properties
            },
            HttpStatusCode.OK);

        await Assert.That(updated.Properties.Any(property => property.Name == $"Host-{suffix}")).IsTrue();
        await DeleteAsync(client, $"/api/ResourceConfiguration/definitions/{definition.Id}", HttpStatusCode.OK);
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task ResourceApis_ShouldValidateNormalizeFilterAndSoftDelete(TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        ResourceFixture fixture = await CreateFixtureAsync(client);

        ResourceCreatedDto created = await PostAsync<ResourceCreatedDto>(
            client,
            "/api/Resource",
            new ResourceAddDto
            {
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                GroupId = fixture.Group.Id,
                DefinitionId = fixture.Definition.Id,
                TagNames = [fixture.Tag.Name, fixture.Tag.Name, "Other"],
                Values = fixture.Values
            },
            HttpStatusCode.Created);
        Guid resourceId = created.Id;

        HttpResponseMessage detailResponse = await client.GetAsync($"/api/Resource/{resourceId}");
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
            "/api/Resource/list?tagName=Other");
        await Assert.That(listResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        JsonDocument list = await listResponse.Content.ReadFromJsonAsync<JsonDocument>()
            ?? throw new InvalidOperationException("Resource list response was empty.");
        await Assert.That(list.RootElement.GetProperty("count").GetInt32()).IsEqualTo(1);
        await Assert.That(list.RootElement.GetProperty("data").EnumerateArray().Single()
            .GetProperty("id").GetGuid()).IsEqualTo(resourceId);

        foreach (string searchKey in new[] { fixture.Definition.Name, fixture.Tag.Name })
        {
            HttpResponseMessage searchResponse = await client.GetAsync(
                $"/api/Resource/list?searchKey={Uri.EscapeDataString(searchKey)}");
            await Assert.That(searchResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
            JsonDocument searchResult = await searchResponse.Content.ReadFromJsonAsync<JsonDocument>()
                ?? throw new InvalidOperationException("Resource search response was empty.");
            await Assert.That(searchResult.RootElement.GetProperty("count").GetInt32()).IsEqualTo(1);
            await Assert.That(searchResult.RootElement.GetProperty("data").EnumerateArray().Single()
                .GetProperty("id").GetGuid()).IsEqualTo(resourceId);
        }

        HttpResponseMessage valueSearchResponse = await client.GetAsync(
            $"/api/Resource/list?environmentId={fixture.Environment.Id}&searchKey=server");
        await Assert.That(valueSearchResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        JsonDocument valueSearchResult = await valueSearchResponse.Content.ReadFromJsonAsync<JsonDocument>()
            ?? throw new InvalidOperationException("Resource value search response was empty.");
        await Assert.That(valueSearchResult.RootElement.GetProperty("count").GetInt32()).IsEqualTo(1);
        await Assert.That(valueSearchResult.RootElement.GetProperty("data").EnumerateArray().Single()
            .GetProperty("id").GetGuid()).IsEqualTo(resourceId);

        HttpResponseMessage noMatchResponse = await client.GetAsync(
            "/api/Resource/list?searchKey=resource-search-no-match");
        await Assert.That(noMatchResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        JsonDocument noMatchResult = await noMatchResponse.Content.ReadFromJsonAsync<JsonDocument>()
            ?? throw new InvalidOperationException("Resource no-match search response was empty.");
        await Assert.That(noMatchResult.RootElement.GetProperty("count").GetInt32()).IsEqualTo(0);

        HttpResponseMessage updateResponse = await PatchAsync(
            client,
            $"/api/Resource/{resourceId}",
            new ResourceAddDto
            {
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
            $"/api/Resource/{resourceId}",
            HttpStatusCode.OK);
        await Assert.That(updated.GroupId).IsNull();
        await Assert.That(updated.TagNames).IsEquivalentTo(["Updated"]);

        await DeleteAsync(client, $"/api/Resource/{resourceId}", HttpStatusCode.OK);
        HttpResponseMessage deletedDetail = await client.GetAsync($"/api/Resource/{resourceId}");
        await Assert.That(deletedDetail.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task PersonalResourceApis_ShouldStorePrivateValuesAndApprovePublicRequests(
        TestHttpClientData data)
    {
        HttpClient client = data.HttpClient;
        ResourceFixture fixture = await CreateFixtureAsync(client);
        PersonalResourceAddDto input = new()
        {
            DefinitionId = fixture.Definition.Id,
            Status = PersonalResourceStatus.Private,
            Values = fixture.Values
        };

        PersonalResourceCreatedDto privateCreated = await PostAsync<PersonalResourceCreatedDto>(
            client,
            "/api/PersonalResource",
            input,
            HttpStatusCode.Created);
        PersonalResourceDetailDto privateDetail = await GetAsync<PersonalResourceDetailDto>(
            client,
            $"/api/PersonalResource/{privateCreated.Id}",
            HttpStatusCode.OK);
        await Assert.That(privateDetail.Status).IsEqualTo(PersonalResourceStatus.Private);
        await Assert.That(privateDetail.AuditStatus).IsEqualTo(PersonalResourceAuditStatus.NotRequired);
        await Assert.That(privateDetail.Values.Select(value => value.Value))
            .IsEquivalentTo(["192.168.0.1", "2026-07-15", "true", "80", "https://example.com/", "server"]);

        PersonalResourceCreatedDto publicCreated = await PostAsync<PersonalResourceCreatedDto>(
            client,
            "/api/PersonalResource",
            new PersonalResourceAddDto
            {
                DefinitionId = fixture.Definition.Id,
                Status = PersonalResourceStatus.ApplyPublic,
                Values = fixture.Values
            },
            HttpStatusCode.Created);
        JsonDocument reviewList = await GetAsync<JsonDocument>(
            client,
            "/api/PersonalResource/review",
            HttpStatusCode.OK);
        await Assert.That(reviewList.RootElement.GetProperty("data").EnumerateArray()
            .Any(item => item.GetProperty("id").GetGuid() == publicCreated.Id)).IsTrue();

        HttpResponseMessage approve = await client.PostAsJsonAsync(
            $"/api/PersonalResource/{publicCreated.Id}/approve",
            new PersonalResourceReviewDto
            {
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                GroupId = fixture.Group.Id,
                TagNames = [fixture.Tag.Name],
                ReviewComment = "Approved"
            });
        await Assert.That(approve.StatusCode).IsEqualTo(HttpStatusCode.OK);

        PersonalResourceDetailDto approved = await GetAsync<PersonalResourceDetailDto>(
            client,
            $"/api/PersonalResource/{publicCreated.Id}",
            HttpStatusCode.OK);
        await Assert.That(approved.AuditStatus).IsEqualTo(PersonalResourceAuditStatus.Approved);
        await Assert.That(approved.ApprovedResourceId).IsNotNull();
        ResourceDetailDto createdResource = await GetAsync<ResourceDetailDto>(
            client,
            $"/api/Resource/{approved.ApprovedResourceId}",
            HttpStatusCode.OK);
        await Assert.That(createdResource.EnvironmentId).IsEqualTo(fixture.Environment.Id);
        await Assert.That(createdResource.CategoryId).IsEqualTo(fixture.Category.Id);
        await Assert.That(createdResource.GroupId).IsEqualTo(fixture.Group.Id);

        await DeleteAsync(client, $"/api/PersonalResource/{privateCreated.Id}", HttpStatusCode.OK);
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
            new ResourceAddDto
            {
                EnvironmentId = fixture.Environment.Id,
                CategoryId = fixture.Category.Id,
                DefinitionId = fixture.Definition.Id,
                Values = []
            });
        await Assert.That(missingRequired.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        ResourceCreatedDto created = await PostAsync<ResourceCreatedDto>(
            client,
            "/api/Resource",
            new ResourceAddDto
            {
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

        HttpResponseMessage removeReferencedProperty = await client.PutAsJsonAsync(
            $"/api/ResourceConfiguration/definitions/{fixture.Definition.Id}",
            new ResDefinitionAddDto
            {
                Name = fixture.Definition.Name,
                Properties = fixture.Definition.Properties
                    .Where(property => property.Id != fixture.AddressProperty.Id)
                    .Select(property => new ResDefinitionPropertyDto
                    {
                        Id = property.Id,
                        Name = property.Name,
                        ValueType = property.ValueType,
                        IsRequired = property.IsRequired,
                        MaxLength = property.MaxLength,
                        Sort = property.Sort
                    })
                    .ToList()
            });
        await Assert.That(removeReferencedProperty.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

        HttpResponseMessage updateSharedProperty = await client.PutAsJsonAsync(
            $"/api/ResourceConfiguration/definitions/{fixture.Definition.Id}",
            new ResDefinitionAddDto
            {
                Name = fixture.Definition.Name,
                Properties = fixture.Definition.Properties
                    .Select(property => new ResDefinitionPropertyDto
                    {
                        Id = property.Id,
                        Name = property.Name == "Address" ? $"Host-{Guid.NewGuid():N}" : property.Name,
                        ValueType = property.ValueType,
                        IsRequired = property.IsRequired,
                        MaxLength = property.MaxLength,
                        Sort = property.Sort
                    })
                    .ToList()
            });
        await Assert.That(updateSharedProperty.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

        await DeleteAsync(client, $"/api/Resource/{created.Id}", HttpStatusCode.OK);
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
            new ResPermissionUpdateDto
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
            new ResPermissionUpdateDto
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

    private static ResDefinitionAddDto DefinitionInput(string suffix)
    {
        return new ResDefinitionAddDto
        {
            Name = $"Definition-{suffix}",
            Properties =
            [
                new() { Name = "Address", ValueType = ResValueType.IPAddress, IsRequired = true, MaxLength = 40, Sort = 2 },
                new() { Name = "Enabled", ValueType = ResValueType.Boolean, IsRequired = true, MaxLength = 10, Sort = 3 },
                new() { Name = "Label", ValueType = ResValueType.String, IsRequired = true, MaxLength = 60, Sort = 6 },
                new() { Name = "TestPort", ValueType = ResValueType.Number, IsRequired = true, MaxLength = 10, Sort = 1 },
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
            new ResEnvironmentAddDto { Name = $"Environment-{suffix}", Color = "#123456" },
            HttpStatusCode.OK);
        ResCategory category = await PostAsync<ResCategory>(
            client,
            "/api/ResourceConfiguration/categories",
            new ResCategoryAddDto
            {
                Name = $"Category-{suffix}",
                CatalogCode = $"catalog-{suffix}",
                Color = "#654321"
            },
            HttpStatusCode.OK);
        ResGroup group = await PostAsync<ResGroup>(
            client,
            "/api/ResourceConfiguration/groups",
            new ResGroupAddDto { Name = $"Group-{suffix}", CategoryId = category.Id, Color = "#abcdef" },
            HttpStatusCode.OK);
        ResTag tag = await PostAsync<ResTag>(
            client,
            "/api/ResourceConfiguration/tags",
            new ResTagAddDto { Name = $"Tag-{suffix}", Color = "#fedcba" },
            HttpStatusCode.OK);
        ResDefinition definition = await PostAsync<ResDefinition>(
            client,
            "/api/ResourceConfiguration/definitions",
            DefinitionInput(suffix),
            HttpStatusCode.OK);

        ResDefinitionProperty address = definition.Properties.Single(property => property.Name == "Address");
        ResDefinitionProperty enabled = definition.Properties.Single(property => property.Name == "Enabled");
        ResDefinitionProperty label = definition.Properties.Single(property => property.Name == "Label");
        ResDefinitionProperty port = definition.Properties.Single(property => property.Name == "TestPort");
        ResDefinitionProperty uri = definition.Properties.Single(property => property.Name == "Uri");
        ResDefinitionProperty when = definition.Properties.Single(property => property.Name == "When");

        return new ResourceFixture(
            environment,
            category,
            group,
            tag,
            definition,
            address,
            [
                new ResourceValueDto { DefinitionPropertyId = address.Id, Value = "192.168.000.001" },
                new ResourceValueDto { DefinitionPropertyId = enabled.Id, Value = "TRUE" },
                new ResourceValueDto { DefinitionPropertyId = label.Id, Value = "server" },
                new ResourceValueDto { DefinitionPropertyId = port.Id, Value = "80" },
                new ResourceValueDto { DefinitionPropertyId = uri.Id, Value = "https://example.com" },
                new ResourceValueDto { DefinitionPropertyId = when.Id, Value = "2026-07-15" }
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
        ResDefinitionProperty AddressProperty,
        List<ResourceValueDto> Values);
}
