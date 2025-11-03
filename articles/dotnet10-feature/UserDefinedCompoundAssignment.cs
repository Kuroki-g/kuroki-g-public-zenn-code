namespace Dotnet10Feature;

/// <summary>
/// ユーザー定義複合割り当て
/// </summary>
/// <see href="https://learn.microsoft.com/ja-jp/dotnet/csharp/language-reference/operators/operator-overloading"/>
class UserDefinedCompoundAssignment
{
    class C1
    {
        public int Value;

        public static C1 operator +(C1 operand) => operand;

        public void operator +=(int x)
        {
            Value += x;
        }
    }
}