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

    builder.Services.AddEndpointsApiExplorer() |> ignore

    builder.Services.AddOpenApiDocument(fun config ->
        config.DocumentName <- "TodoAPI"
        config.Title <- "TodoAPI v1"
        config.Version <- "v1")
    |> ignore

    let app = builder.Build()

    if app.Environment.IsDevelopment() then
        app.UseOpenApi() |> ignore

        app.UseSwaggerUi(fun config ->
            config.DocumentTitle <- "TodoAPI"
            config.Path <- "/swagger"
            config.DocumentPath <- "/swagger/{documentName}/swagger.json"
            config.DocExpansion <- "list")
        |> ignore

    let todoItems = app.MapGroup "/todoitems"

    todoItems.MapGet("/", Func<TodoDb, Task<Collections.Generic.List<Todo>>>(fun db -> db.Todos.ToListAsync()))
    |> ignore

    todoItems.MapGet(
        "/complete",
        Func<TodoDb, Task<Collections.Generic.List<Todo>>>(fun db ->
            db.Todos.Where(fun t -> t.IsComplete).ToListAsync())
    )
    |> ignore

    todoItems.MapGet(
        "/{id}",
        Func<int, TodoDb, Task<IResult>>(fun id db ->
            task {
                match! db.Todos.FindAsync id with
                | null -> return Results.NotFound()
                | todo -> return Results.Ok todo
            })
    )
    |> ignore

    todoItems.MapPost(
        "/",
        Func<Todo, TodoDb, Task<IResult>>(fun todo db ->
            task {
                db.Todos.Add todo |> ignore
                let! result = db.SaveChangesAsync()

                return Results.Created($"/todoitems/{todo.Id}", result)
            })
    )
    |> ignore

    todoItems.MapPut(
        "/{id}",
        Func<int, Todo, TodoDb, Task<IResult>>(fun id inputTodo db ->
            task {
                match! db.Todos.FindAsync id with
                | null -> return Results.NotFound()
                | todo ->
                    todo.Name <- inputTodo.Name
                    todo.IsComplete <- inputTodo.IsComplete
                    db.SaveChangesAsync() |> ignore
                    return Results.NoContent()
            })
    )
    |> ignore

    todoItems.MapDelete(
        "/{id}",
        Func<int, TodoDb, Task<IResult>>(fun id db ->
            task {
                match! db.Todos.FindAsync id with
                | null -> return Results.NotFound()
                | todo ->
                    db.Todos.Remove todo |> ignore
                    db.SaveChangesAsync() |> ignore
                    return Results.NoContent()
            })
    )
    |> ignore

    app.Run()

    0 // Exit code
