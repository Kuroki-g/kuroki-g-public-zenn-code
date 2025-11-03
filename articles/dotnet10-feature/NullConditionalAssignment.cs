namespace Dotnet10Feature;

/// <summary>
/// Null 条件付き割り当て
/// </summary>
/// <see href="https://learn.microsoft.com/ja-jp/dotnet/csharp/whats-new/csharp-14#null-conditional-assignment"/>
class NullConditionalAssignment
{
    void NullAssign(Customer? customer)
    {
        // Null check can be simplified (IDE0031) が出るようになりました。
        if (customer is not null)
        {
            customer.Order = GetCurrentOrder();
        }

        customer?.Order = GetCurrentOrder();
    }

    class Customer
    {
        public string? Order { get; set; } = null;
    }

    static string GetCurrentOrder()
    {
        return "CurrentOrder";
    }
}
