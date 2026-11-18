namespace Perigon.AspNetCore.Models;

/// <summary>
/// 过滤
/// </summary>
public class FilterBase
{
    public int? PageIndex
    {
        get;
        set { field = value is null or < 1 ? 1 : value; }
    } = 1;

    /// <summary>
    /// max 10000
    /// </summary>
    public int? PageSize
    {
        get;
        set { field = value is null ? 20 : value > 10000 ? 10000 : value < 0 ? 0 : value; }
    } = 20;

}

