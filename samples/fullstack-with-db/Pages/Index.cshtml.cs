using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace exampleapp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public string DbResult { get; set; }

        public void OnGet()
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");

            if (connectionString == null)
            {
                DbResult = "CONNECTIONSTRING environment variable not set, could not connect to DB";
            }
            else
            {
                try
                {

                    using var connection = new NpgsqlConnection(connectionString);
                    connection.Open();

                    using var dropTableCommand = new NpgsqlCommand(
                        @"DROP TABLE IF EXISTS example_table;", connection);
                    dropTableCommand.ExecuteNonQuery();

                    using var createTableCommand = new NpgsqlCommand(
                        @"CREATE TABLE example_table (
                       id serial PRIMARY KEY,
                       value text NOT NULL
                    );", connection);
                    createTableCommand.ExecuteNonQuery();

                    var valueToInsert = Guid.NewGuid().ToString();
                    using var insertDataCommand = new NpgsqlCommand(
                        $"INSERT INTO example_table (value) VALUES ('{valueToInsert}');", connection);
                    insertDataCommand.ExecuteNonQuery();

                    DbResult = connection.QueryFirst<string>("SELECT value from example_table");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "failed when accessing DB");
                    DbResult = "Error when accessing DB: " + e.ToString();
                }
            }
        }
    }
}
