using DuckDB.NET.Data;

using var duckDBConnection = new DuckDBConnection("Data Source=./data/file.db");
duckDBConnection.Open();

using var command = duckDBConnection.CreateCommand();

command.CommandText = "CREATE TABLE sample_database AS SELECT * FROM read_xlsx('./data/05k2-3.xlsx', header = true, range = 'A4:I100', empty_as_varchar=true);";
var executeNonQuery = command.ExecuteNonQuery();

command.CommandText = "SELECT * FROM sample_database";
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
      var val = queryResult.GetValue(ordinal).ToString();
      Console.Write(val);
      Console.Write(" ");
    }

    Console.WriteLine();
  }
}
