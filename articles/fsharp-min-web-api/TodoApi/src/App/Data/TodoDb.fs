namespace TodoApi.Data

open Microsoft.EntityFrameworkCore
open TodoApi.Models

type TodoDb(options: DbContextOptions<TodoDb>) =
    inherit DbContext(options)

    member _.Todos = base.Set<Todo>()
