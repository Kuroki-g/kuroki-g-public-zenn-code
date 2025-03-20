namespace TodoApi.Models

type Todo() =
    member val Id: int = 0 with get, set
    member val Name: string | null = null with get, set
    member val IsComplete: bool = false with get, set
    member val Secret: string option = None with get, set
