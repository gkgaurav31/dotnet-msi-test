using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using Npgsql;
using Azure.Identity;
using System.Threading;

namespace TestApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public static string Host = "rolfpsqldb.postgres.database.azure.com";
        public static string dbuser = "myuser@rolfpsqldb";
        public static string Database = "postgres";
        //private static string ClientId = "CLIENT_ID";

        public static string status = "Unknown";

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get() {
            
            test();
            Thread.Sleep(1000 * 10);
            return status;
        }

        async public void test()
        {

            //start

            //
            // Get an access token for PostgreSQL.
            //
            Console.Out.WriteLine("Getting access token from Azure AD...");

            // Azure AD resource ID for Azure Database for PostgreSQL is https://ossrdbms-aad.database.windows.net/
            string accessToken = null;

            try
            {
                // Call managed identities for Azure resources endpoint.
                var sqlServerTokenProvider = new DefaultAzureCredential();
                accessToken = (await sqlServerTokenProvider.GetTokenAsync(
                    new Azure.Core.TokenRequestContext(scopes: new string[] { "https://ossrdbms-aad.database.windows.net/.default" }) { })).Token;
                WeatherForecastController.status = "End try";
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceInformation("There was an exception");
                Console.WriteLine("There was an exception - gaukk");
                WeatherForecastController.status = "In catch";
                Console.Out.WriteLine("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : "Acquire token failed");
                //System.Environment.Exit(1);
            }

            //
            // Open a connection to the PostgreSQL server using the access token.
            //

            WeatherForecastController.status = "After try/catch";

            string connString =
                String.Format(
                    "Server={0}; User Id={1}; Database={2}; Port={3}; Password={4}; SSLMode=Prefer",
                    Host,
                    dbuser,
                    Database,
                    5432,
                    accessToken);

            WeatherForecastController.status = "Trying to connect";

            using (var conn = new NpgsqlConnection(connString))
            {
                Console.Out.WriteLine("Opening connection using access token...");
                Console.WriteLine("Connected - gaukk");
                System.Diagnostics.Trace.TraceInformation("Opening connection using access token...");
                conn.Open();

                WeatherForecastController.status = "Connected";

                using (var command = new NpgsqlCommand("SELECT version()", conn))
                {

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine("\nConnected!\n\nPostgres version: {0}", reader.GetString(0));
                        WeatherForecastController.status = reader.GetString(0);
                        System.Diagnostics.Trace.TraceInformation("Connected - gauk");
                    }
                }
            }


            //end
        }



    }
}
