using Entity;
namespace SystemMod;

/// <summary>
/// The enum catalog exposed by the system configuration endpoint.
///
/// Keep this list explicit: the previous assembly scan was not compatible with
/// trimming or Native AOT and did not include enums outside Share.dll/Entity.dll.
/// </summary>
internal static class SystemEnumCatalog
{
    public static Dictionary<string, List<EnumDictionary>> Create(
        IReadOnlyDictionary<string, List<EnumDictionary>> externalEntries
    )
    {
        Dictionary<string, List<EnumDictionary>> catalog = EnumHelper.CreateEnumInfo(
            // SystemMod owns these enums. Other modules pass their own entries
            // at the endpoint boundary.
            (nameof(DataScopeType), EnumHelper.ToList<DataScopeType>()),
            (nameof(MenuType), EnumHelper.ToList<MenuType>(
                (MenuType.Page, "页面"),
                (MenuType.Button, "按钮"))),
            (nameof(Sex), EnumHelper.ToList<Sex>(
                (Sex.Male, "男性"),
                (Sex.Female, "女性"),
                (Sex.Else, "其他")))
        );

        foreach ((string name, List<EnumDictionary> values) in externalEntries)
        {
            if (!catalog.TryAdd(name, values))
            {
                throw new InvalidOperationException(
                    $"The enum catalog already contains an entry named '{name}'.");
            }
        }

        return catalog;
    }
}
