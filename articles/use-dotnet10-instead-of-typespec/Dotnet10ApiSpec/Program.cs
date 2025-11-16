using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();
app.MapOpenApi("/openapi/{documentName}.yaml");

app.UseHttpsRedirection();
app.MapControllers();

[JsonConverter(typeof(JsonStringEnumConverter<WidgetColor>))]
public enum WidgetColor
{
    Red,
    Blue
}

public record Widget(
 string Id,
 int Weight,
 WidgetColor Color
);

public record WidgetList(
  Widget[] Items
);

public record AnalyzeResult(
  string Id,
    string Analysis
);

[ApiController]
[Route("/widgets")]
[Tags("Widgets")]
[Produces("application/json")]
public class WidgetsController : Controller
{
    /// <summary>
    /// List widgets
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet]
    public WidgetList List()
    {
        throw new NotImplementedException();
    }

    /** Read widgets */
    [HttpGet]
    [Route("{id:string}")]
    public Widget Read([FromRoute] string id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a widget
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPost]
    public Widget Create([FromBody] Widget body)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update a widget
    /// </summary>
    /// <param name="id"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPatch]
    [Route("{id:string}")]
    public Widget Update(
        [FromRoute] string id,
        [FromBody] Widget body)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete a widget
    /// </summary>
    /// <param name="id"></param>
    /// <param name=""></param>
    /// <returns></returns>
    [HttpDelete]
    public void Delete([FromRoute] string id)
    {
        throw new NotImplementedException();
    }

    /** Analyze a widget */
    [HttpPost("{id}/analyze")]
    public AnalyzeResult Analyze([FromRoute] string id)
    {
        throw new NotImplementedException();
    }
}
