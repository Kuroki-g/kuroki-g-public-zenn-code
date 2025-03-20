open System
open System.Linq
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.EntityFrameworkCore
open System.Threading.Tasks
open TodoApi.Data
open TodoApi.Dtos
open TodoApi.Models

module TodoItems =
    let getAllTodos =
        Func<TodoDb, Task<IResult>>(fun db ->
            task { return db.Todos.Select(TodoItemDTO.Create).ToListAsync() |> TypedResults.Ok :> IResult })

    let getCompleteTodos =
        Func<TodoDb, Task<IResult>>(fun db ->
            task {
                return
                    db.Todos.Where(fun t -> t.IsComplete).Select(TodoItemDTO.Create).ToListAsync()
                    |> TypedResults.Ok
                    :> IResult
            })

    let getTodo =
        Func<int, TodoDb, Task<IResult>>(fun id db ->
            task {
                match! db.Todos.FindAsync id with
                | null -> return TypedResults.NotFound() :> IResult
                | todo -> return todo |> TodoItemDTO.Create |> TypedResults.Ok :> IResult
            })

    let createTodo =
        Func<TodoItemDTO, TodoDb, Task<IResult>>(fun todoItemDTO db ->
            task {
                let todoItem = new Todo()
                todoItem.Name <- todoItemDTO.Name
                todoItem.IsComplete <- todoItemDTO.IsComplete
                db.Todos.Add todoItem |> ignore
                let! result = db.SaveChangesAsync()

                return TypedResults.Created($"/todoitems/{todoItem.Id}", result) :> IResult
            })

    let updateTodo =
        Func<int, TodoItemDTO, TodoDb, Task<IResult>>(fun id todoItemDTO db ->
            task {
                match! db.Todos.FindAsync id with
                | null -> return TypedResults.NotFound() :> IResult
                | todo ->
                    todo.Name <- todoItemDTO.Name
                    todo.IsComplete <- todoItemDTO.IsComplete
                    db.SaveChangesAsync() |> ignore
                    return TypedResults.NoContent() :> IResult
            })

    let deleteTodo =
        Func<int, TodoDb, Task<IResult>>(fun id db ->
            task {
                match! db.Todos.FindAsync id with
                | null -> return TypedResults.NotFound() :> IResult
                | todo ->
                    db.Todos.Remove todo |> ignore
                    db.SaveChangesAsync() |> ignore
                    return TypedResults.NoContent() :> IResult
            })

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

    todoItems.MapGet("/", TodoItems.getAllTodos) |> ignore
    todoItems.MapGet("/complete", TodoItems.getCompleteTodos) |> ignore
    todoItems.MapGet("/{id}", TodoItems.getTodo) |> ignore
    todoItems.MapPost("/", TodoItems.createTodo) |> ignore
    todoItems.MapPut("/{id}", TodoItems.updateTodo) |> ignore
    todoItems.MapDelete("/{id}", TodoItems.deleteTodo) |> ignore

    app.Run()

    0 // Exit code
