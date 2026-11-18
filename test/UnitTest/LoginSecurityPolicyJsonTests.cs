using Perigon.AspNetCore.Options;
using ApiService.Serialization;

namespace UnitTest;

public sealed class LoginSecurityPolicyJsonTests
{
    [Test]
    public async Task RoundTrip_PreservesAllPolicyFields()
    {
        var expected = new LoginSecurityPolicyOption
        {
            PasswordLevel = PasswordLevel.Strict,
            IsNeedVerifyCode = true,
            PasswordExpired = 90,
            LoginRetry = 3,
            SessionLevel = SessionLevel.OnlyOne,
            SessionExpiredSeconds = 600,
            IsEnable = true,
        };

        var serializer = new AppJsonSerializer();
        LoginSecurityPolicyOption actual = serializer.Deserialize<LoginSecurityPolicyOption>(serializer.Serialize(expected))!;

        await Assert.That(actual.PasswordLevel).IsEqualTo(expected.PasswordLevel);
        await Assert.That(actual.IsNeedVerifyCode).IsEqualTo(expected.IsNeedVerifyCode);
        await Assert.That(actual.PasswordExpired).IsEqualTo(expected.PasswordExpired);
        await Assert.That(actual.LoginRetry).IsEqualTo(expected.LoginRetry);
        await Assert.That(actual.SessionLevel).IsEqualTo(expected.SessionLevel);
        await Assert.That(actual.SessionExpiredSeconds).IsEqualTo(expected.SessionExpiredSeconds);
        await Assert.That(actual.IsEnable).IsEqualTo(expected.IsEnable);
    }

    [Test]
    public async Task Deserialize_MissingFieldsUsesPolicyDefaults()
    {
        LoginSecurityPolicyOption actual = new AppJsonSerializer().Deserialize<LoginSecurityPolicyOption>("{}")!;

        await Assert.That(actual.PasswordLevel).IsEqualTo(PasswordLevel.Normal);
        await Assert.That(actual.PasswordExpired).IsEqualTo(365);
        await Assert.That(actual.LoginRetry).IsEqualTo(5);
        await Assert.That(actual.SessionLevel).IsEqualTo(SessionLevel.None);
        await Assert.That(actual.SessionExpiredSeconds).IsEqualTo(1800);
        await Assert.That(actual.IsNeedVerifyCode).IsFalse();
        await Assert.That(actual.IsEnable).IsFalse();
    }
}
