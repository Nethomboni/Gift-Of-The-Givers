using GiftOfTheGivers.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GiftOfTheGivers.Tests.Integration.Support
{
    // Starts the REAL web app. Only the database is swapped: Azure SQL / LocalDB is
    // replaced by an isolated in-memory SQLite database (one per test class), so
    // the tests never touch the Azure database. Nothing else is replaced: the
    // real FunctionsApiClient still calls the real Azure Functions.
    public class GotgWebFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        public GotgWebFactory()
        {
            // Program.cs throws if no "DefaultConnection" exists. The value is never
            // used, because the DbContext is replaced below.
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "DataSource=:memory:");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // The in-memory database exists only while this connection is open.
            _connection.Open();

            builder.ConfigureTestServices(services =>
            {
                // 1. Remove the real SQL Server DbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 2. Register the isolated SQLite test database.
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));

                // 3. Create the tables now. Program.cs seeds data right after startup
                //    and would fail with "no such table" if the schema did not exist.
                using var context = new ApplicationDbContext(
                    new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
                context.Database.EnsureCreated();
            });
        }

        // Runs a query directly against this factory's test database.
        public T WithDb<T>(Func<ApplicationDbContext, T> query)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return query(db);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }
}