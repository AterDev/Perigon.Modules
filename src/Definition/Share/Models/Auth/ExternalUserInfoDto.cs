namespace Share.Models.Auth;

/// <summary>Identity returned by an external authentication provider.</summary>
public sealed class ExternalUserInfoDto
{
    public string? Email { get; set; }
    public string? Name { get; set; }
}
