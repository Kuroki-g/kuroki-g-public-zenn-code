namespace Dotnet10Feature;

/// <summary>
/// バインドされていないジェネリック型と nameof
/// </summary>
/// <see href="https://learn.microsoft.com/ja-jp/dotnet/csharp/whats-new/csharp-14#unbound-generic-types-and-nameof"/> 
public static class UnboundGenericTypesAndNameof
{
    public static void ShowExample()
    {
        // C# 14 以降では、 nameof する引数はバインドされていないジェネリック型にすることができます。
        var nameofList = nameof(List<>);
        // 以前のバージョンでは閉じたジェネリック型のみが使用可能でした。
        // NOTE: Use unbound generic type (IDE0340) の警告が出ます。
        var nameofListInt = nameof(List<int>);
    }
}
