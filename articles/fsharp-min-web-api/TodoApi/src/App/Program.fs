open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.EntityFrameworkCore
open TodoApi.Data

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddDbContext<TodoDb>(fun opt -> opt.UseInMemoryDatabase("TodoList") |> ignore)
    |> ignore

    builder.Services.AddDatabaseDeveloperPageExceptionFilter() |> ignore

    let app = builder.Build()

    app.MapGet("/", Func<string>(fun () -> "Hello World!")) |> ignore

    app.Run()

    0 // Exit code
