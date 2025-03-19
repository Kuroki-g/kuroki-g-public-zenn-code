using CsharpApp.Models;

using Microsoft.EntityFrameworkCore;

namespace CsharpApp.Data;

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}
