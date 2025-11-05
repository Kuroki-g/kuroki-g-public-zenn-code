using System.Text.Json;

namespace Dotnet10Feature;

public static class Serializations
{
    record MyRecord(int Value);

    /// <summary>
    /// JSON プロパティの重複を禁止するオプション
    /// </summary>
    /// <see href="https://learn.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-10/libraries#strict-json-serialization-options"/>
    public static void OptionToDisallowDuplicateJsonProperties()
    {
        string json = """{ "Value": 1, "Value": -1 }""";
        Console.WriteLine(JsonSerializer.Deserialize<MyRecord>(json)?.Value); // -1

        // AllowDuplicatePropertiesを指定すると、例外となります。
        JsonSerializerOptions options = new() { AllowDuplicateProperties = false };
        try
        {
            // JsonObject、Dictionary<string, int>でも同様にJsonExceptionとなります。
            JsonSerializer.Deserialize<MyRecord>(json, options);
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex.Message);
        }

        // JsonDocumentでも同様に、AllowDuplicatePropertiesの設定が可能です。
        JsonDocumentOptions docOptions = new() { AllowDuplicateProperties = false };
        try
        {
            JsonDocument.Parse(json, docOptions);
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    /// <summary>
    /// 厳密な JSON シリアル化オプション
    /// </summary>
    /// <see href="https://learn.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-10/libraries#strict-json-serialization-options"/>
    public static void StrictJsonSerializationOptions()
    {
        string json = """{ "Value": 1, "Value": -1 }""";
        JsonSerializer.Deserialize<MyRecord>(json, JsonSerializerOptions.Strict);
        // JsonSerializerOptions.Strict プリセットが追加されました。
        // これは、以下のものに等しいです。
        // JsonUnmappedMemberHandling.Disallow
        // + JsonSerializerOptions.AllowDuplicateProperties = false
        // + case sensitive (大文字と小文字の区別)
        // + JsonSerializerOptions.RespectNullableAnnotations
        // + JsonSerializerOptions.RespectRequiredConstructorParameters
        // <https://github.com/dotnet/dotnet/blob/89c8f6a112d37d2ea8b77821e56d170a1bccdc5a/src/runtime/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.cs#L180>
    }
}