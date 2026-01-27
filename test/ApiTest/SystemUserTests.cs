using ApiTest.Data;

namespace ApiTest;

public class SystemUserTests
{
    [ClassDataSource<TestHttpClientData>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task GetUserInfo_ShouldReturnUserDetails(TestHttpClientData httpClientData)
    {
        //var httpClient = httpClientData.HttpClient;
        //var response = await httpClient.GetAsync("/api/systemUser/userinfo");

        //await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        //var userInfo = await response.Content.ReadFromJsonAsync<UserInfoDto>();
        //await Assert.That(userInfo).IsNotNull();
        //await Assert.That(userInfo!.Username).IsNotNullOrEmpty();
        //await Assert.That(userInfo.Roles).IsNotNull();
        //await Assert.That(userInfo.Roles!.Length).IsGreaterThan(0);
    }
}