using DuckDB.NET.Data;

using var duckDBConnection = new DuckDBConnection("Data Source=./data/file.db");
duckDBConnection.Open();

using var command = duckDBConnection.CreateCommand();

command.CommandText = "CREATE TABLE integers(foo INTEGER, bar INTEGER);";
var executeNonQuery = command.ExecuteNonQuery();

command.CommandText = "INSERT INTO integers VALUES (3, 4), (5, 6), (7, 8);";
executeNonQuery = command.ExecuteNonQuery();

command.CommandText = "Select count(*) from integers";
var executeScalar = command.ExecuteScalar();

command.CommandText = "SELECT foo, bar FROM integers";
var reader = command.ExecuteReader();

PrintQueryResults(reader);

static void PrintQueryResults(DuckDBDataReader queryResult)
{
  for (var index = 0; index < queryResult.FieldCount; index++)
  {
    var column = queryResult.GetName(index);
    Console.Write($"{column} ");
  }

  Console.WriteLine();

  while (queryResult.Read())
  {
    for (int ordinal = 0; ordinal < queryResult.FieldCount; ordinal++)
    {
      var val = queryResult.GetInt32(ordinal);
      Console.Write(val);
      Console.Write(" ");
    }

    Console.WriteLine();
  }
}
