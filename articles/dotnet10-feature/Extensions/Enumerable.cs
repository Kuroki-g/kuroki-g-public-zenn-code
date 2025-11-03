namespace Dotnet10Feature.Extensions;

/// <summary>
/// 拡張メンバー
/// C# 14 では、 拡張メンバーを定義するための新しい構文が追加されています。 
/// </summary>
/// <see href="https://learn.microsoft.com/ja-jp/dotnet/csharp/whats-new/csharp-14#extension-members"/> 
public static class Enumerable
{
    // 新しく、extensionブロックを用いて拡張メンバーを宣言することができるようになりました。
    // Extension block
    // https://learn.microsoft.com/ja-jp/dotnet/csharp/programming-guide/classes-and-structs/extension-methods#declare-extension-members
    extension<TSource>(IEnumerable<TSource> source) // extension members for IEnumerable<TSource>
    {
        // 新しく拡張プロパティの宣言ができるようになりました:
        public bool IsEmpty => !source.Any();

        // 拡張メソッドの宣言をextensionブロックに書くことができます:
        public IEnumerable<TSource> Where(Func<TSource, bool> predicate)
        {
            throw new NotImplementedException();
        }
    }

    // C# 14より前の場合、拡張プロパティはないのでメソッドで宣言する必要があります。
    public static bool BeforeC14IsEmpty<TSource>(this IEnumerable<TSource> source)
        => !source.Any();

    // 既存のC# 14より前のバージョンの拡張メソッドの宣言はこれまで通りです。
    public static IEnumerable<TSource> BeforeC14Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        throw new NotImplementedException();
    }

    // 静的メンバー + operatorのみの場合には、レシーバー型のみの、extensionブロックで表現することもできる。
    extension<TSource>(IEnumerable<TSource>)
    {
        // static拡張メソッド:
        public static IEnumerable<TSource> Combine(IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            throw new NotImplementedException();
        }

        // static拡張プロパティが定義可能です:
        public static IEnumerable<TSource> Identity => System.Linq.Enumerable.Empty<TSource>();

        // ユーザー定義のoperatorの定義ができるようになりました:
        // NOTE: [Extensions (拡張型) 未確認飛行 C]<https://ufcpp.net/blog/2024/3/extensions/> を見るに、しばらく前から要望があったようです。
        public static IEnumerable<TSource> operator +(IEnumerable<TSource> left, IEnumerable<TSource> right) => left.Concat(right);
    }
}
