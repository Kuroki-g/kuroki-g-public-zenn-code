namespace TodoApi.Dtos

open TodoApi.Models

type TodoItemDTO =
    { Id: int
      Name: string | null
      IsComplete: bool }

    static member Create(v: Todo) =
        { Id = v.Id
          Name = v.Name
          IsComplete = v.IsComplete }
