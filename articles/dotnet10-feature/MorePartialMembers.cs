namespace Dotnet10Feature;

public partial class MorePartialMembers
{
    // partial classが非常に長くなる場合に、このようにメソッドのみの宣言を事前にしておくことが可能です。
    // NOTE: staticは不可です。
    partial void PartialMethod(string s);

    // partial classで部分コンストラクターの宣言が可能になりました。
    // https://learn.microsoft.com/ja-jp/dotnet/csharp/programming-guide/classes-and-structs/constructors#partial-constructors
    public partial MorePartialMembers();
}

public partial class MorePartialMembers
{
    partial void PartialMethod(string s) => Console.WriteLine($"Something happened: {s}");

    public partial MorePartialMembers() // base()又はthis()の使用をする場合には、こちらに追加する必要があります。
    {
        // ここに実装宣言に追加することができます。   
    }
}
