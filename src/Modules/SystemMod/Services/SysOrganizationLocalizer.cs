using System.Resources;

namespace SystemMod.Services;

internal static class SysOrganizationLocalizer
{
    private static readonly ResourceManager Messages = new(
        "SystemMod.Resources.SysOrganizationMessages",
        typeof(SysOrganizationLocalizer).Assembly);

    public static string ParentCycle => Messages.GetString(nameof(ParentCycle))!;
    public static string HasChildren => Messages.GetString(nameof(HasChildren))!;
    public static string HasUsers => Messages.GetString(nameof(HasUsers))!;
    public static string NameExists => Messages.GetString(nameof(NameExists))!;
    public static string NameRequired => Messages.GetString(nameof(NameRequired))!;
}
