namespace Dotnet10Feature;

/// <summary>
/// field キーワード
/// </summary>
/// <see href="https://learn.microsoft.com/ja-jp/dotnet/csharp/whats-new/csharp-14#the-field-keyword"/>
class FieldFeature
{
    #region 
    // fieldキーワードを使用した新しい形式です。
    public string NewFormatProperty
    {
        get;
        // fieldキーワードを使うと簡略化することができます。
        // WARNING: `field`という名前のシンボルを含む型のコードがある場合には、ワークアラウンドが必要です。
        // NOTE: null許容参照の警告が出ます。
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    // 古い形式のプロパティ。VSCodeは、infoレベルでの警告となります。
    // 自動的に新しい形式に変更することが可能です。
    // NOTE: null許容参照の警告が出ます。
    private string _msg; // この形式の場合には、バッキングフィールドの宣言が必要です。

    public string OldFormatProperty
    {
        get => _msg;
        set => _msg = value ?? throw new ArgumentNullException(nameof(value));
    }
    #endregion
}
