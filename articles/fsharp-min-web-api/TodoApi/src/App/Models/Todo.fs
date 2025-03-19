namespace TodoApi.Models

[<CLIMutableAttribute>]
type Todo =
    { Id: int
      mutable Name: string | null
      mutable IsComplete: bool }
