namespace Dotnet10Feature;

/// <summary>
/// 文字列比較の数値の順序付け
/// </summary>
/// <see href="https://learn.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-10/libraries#numeric-ordering-for-string-comparison"/>
public static class NumericOrderingForStringComparison
{
    public static void Main()
    {
        StringComparer numericStringComparer = StringComparer.Create(
            System.Globalization.CultureInfo.CurrentCulture,
            System.Globalization.CompareOptions.NumericOrdering // NumericOrderingオプションが追加されました。
        );

        Console.WriteLine(numericStringComparer.Equals("02", "2")); // これはTrueと判定されます。

        var sorted = new[] { "Windows XP", "Windows 10", "Windows 8", "Windows 11" }.Order(numericStringComparer);
        // 10、11の数字が考慮され、以下の順番で出力されます:
        // Windows 8
        // Windows 10
        // Windows 11
        // Windows XP
        foreach (string os in sorted)
        {
            Console.WriteLine(os);
        }

        HashSet<string> set = new(numericStringComparer) { "007", "07", "7" };
        Console.WriteLine(string.Join(",", set.ToArray())); // Output: 007 <- 順番が考慮されます。
        Console.WriteLine(set.Contains("7")); // Output: True

        HashSet<string> set2 = new(numericStringComparer) { "7", "007", "07",  };
        Console.WriteLine(string.Join(",", set2.ToArray())); // Output: 7 <- 順番が考慮されます。
        Console.WriteLine(set2.Contains("007")); // Output: True
    }
}