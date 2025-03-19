open System
open System.Linq
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.EntityFrameworkCore
open System.Threading.Tasks
open TodoApi.Data
open TodoApi.Models

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddDbContext<TodoDb>(fun opt -> opt.UseInMemoryDatabase("TodoList") |> ignore)
    |> ignore

    builder.Services.AddDatabaseDeveloperPageExceptionFilter() |> ignore

    let app = builder.Build()

    app.MapGet("/todoitems", Func<TodoDb, Task<Collections.Generic.List<Todo>>>(fun db -> db.Todos.ToListAsync()))
    |> ignore

    app.MapGet(
        "/todoitems/complete",
        Func<TodoDb, Task<Collections.Generic.List<Todo>>>(fun db ->
            db.Todos.Where(fun t -> t.IsComplete).ToListAsync())
    )
    |> ignore

    app.MapGet(
        "/todoitems/{id}",
        Func<int, TodoDb, Task<IResult>>(fun id db ->
            task {
                match! db.Todos.FindAsync id with
                | null -> return Results.NotFound()
                | todo -> return Results.Ok todo
            })
    )
    |> ignore

    app.Run()

    0 // Exit code
