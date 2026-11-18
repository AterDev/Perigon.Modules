using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Perigon.AspNetCore.Utils;

/// <summary>
/// 枚举帮助类
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// 枚举转数组
    /// </summary>
    /// <returns></returns>
    [RequiresUnreferencedCode("Enum field metadata and DescriptionAttribute values must be preserved.")]
    [RequiresDynamicCode("Enum values are requested through a runtime Type.")]
    public static List<EnumDictionary> ToList(Type type)
    {
        List<EnumDictionary> result = [];
        var enumNames = Enum.GetNames(type);
        var values = Enum.GetValues(type);

        if (enumNames != null)
        {
            for (var i = 0; i < enumNames.Length; i++)
            {
                var field = type.GetField(enumNames[i], BindingFlags.Public | BindingFlags.Static);
                if (field != null)
                {
                    var attribute = field.GetCustomAttribute<DescriptionAttribute>(true);
                    result.Add(
                        new EnumDictionary
                        {
                            Name = field.Name,
                            Description = attribute?.Description ?? field.Name,
                            Value = Convert.ToInt32(values.GetValue(i)),
                        }
                    );
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Gets enum information through the generic enum API. This overload is safe for
    /// Native AOT and uses the supplied descriptions instead of reflecting over fields.
    /// </summary>
    public static List<EnumDictionary> ToList<TEnum>(
        params (TEnum Value, string Description)[] descriptions
    ) where TEnum : struct, Enum
    {
        Dictionary<TEnum, string> descriptionByValue = descriptions.ToDictionary(
            item => item.Value,
            item => item.Description
        );
        List<EnumDictionary> result = [];
        foreach (TEnum value in Enum.GetValues<TEnum>())
        {
            string name = value.ToString();
            result.Add(new EnumDictionary
            {
                Name = name,
                Description = descriptionByValue.GetValueOrDefault(value, name),
                Value = Convert.ToInt32(value),
            });
        }
        return result;
    }

    /// <summary>
    /// 获取枚举描述
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Enum field metadata and DescriptionAttribute values must be preserved.")]
    public static string GetDescription(this Enum value)
    {
        var field = value
            .GetType()
            .GetField(value.ToString(), BindingFlags.Public | BindingFlags.Static);
        return
            field != null
            && field.GetCustomAttribute<DescriptionAttribute>(true)
                is DescriptionAttribute attribute
            ? attribute.Description
            : value.ToString();
    }

    /// <summary>
    /// 获取程序集中所有枚举类型
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Assembly enum types may be removed by trimming.")]
    public static Type[] GetEnumTypes(this Assembly assembly)
    {
        return assembly.GetTypes().Where(t => t.IsEnum).ToArray();
    }

    /// <summary>
    /// 获取程序集所有枚举信息
    /// </summary>
    /// <returns></returns>
    [RequiresUnreferencedCode("Assembly scanning and enum field metadata are not trim-safe.")]
    [RequiresDynamicCode("Enum values are requested through runtime Type instances.")]
    public static Dictionary<string, List<EnumDictionary>> GetAllEnumInfo(
        List<string>? myAssemblies = null
    )
    {
        Dictionary<string, List<EnumDictionary>> res = [];
        myAssemblies ??= ["Share.dll", "Entity.dll"];

        List<Type> allTypes = [];
        var assemblies = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => myAssemblies.Contains($"{a.GetName().Name}.dll"))
            .ToList();
        assemblies.ForEach(assembly =>
        {
            Type[] types = assembly.GetEnumTypes();
            allTypes.AddRange(types);
        });

        foreach (Type type in allTypes)
        {
            List<EnumDictionary> enumDict = ToList(type);
            res.Add(type.Name, enumDict);
        }
        return res;
    }

    /// <summary>
    /// Creates an enum catalog from statically registered enum entries.
    /// </summary>
    public static Dictionary<string, List<EnumDictionary>> CreateEnumInfo(
        params (string Name, List<EnumDictionary> Values)[] enumInfos
    )
    {
        Dictionary<string, List<EnumDictionary>> result = [];
        foreach ((string name, List<EnumDictionary> values) in enumInfos)
        {
            result.Add(name, values);
        }
        return result;
    }
}

public class EnumDictionary
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int Value { get; set; }
}
