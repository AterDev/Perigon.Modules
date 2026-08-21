using ApiTest.Data;
using Perigon.AspNetCore.Constants;
using SystemMod.Models;
using System.Net.Http.Json;

namespace ApiTest;

public class SystemUserTests
{
    [ClassDataSource<TestHttpClientData>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task GetUserInfo_ShouldReturnUserDetails(TestHttpClientData httpClientData)
    {
        var response = await httpClientData.HttpClient.GetAsync("/api/systemUser/userinfo");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var userInfo = await response.Content.ReadFromJsonAsync<UserInfoDto>();
        await Assert.That(userInfo).IsNotNull();
        await Assert.That(userInfo!.Username).IsEqualTo("admin");
        await Assert.That(userInfo.Roles).IsNotNull();
        await Assert.That(userInfo.Roles!).Contains(WebConst.SuperAdmin);
    }
}
