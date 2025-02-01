using Microsoft.AspNetCore.Mvc.Testing;

namespace TodoApi.Web.ApiTest.Tests;

public class BasicTests(WebApplicationFactory<Program> factory)
        : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Theory]
    [InlineData("/")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var contentType = response.Content.Headers.ContentType?.ToString();
        Assert.Equal("text/plain; charset=utf-8", contentType);
    }
}