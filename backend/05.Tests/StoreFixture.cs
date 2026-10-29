using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;
namespace ClothingStore.Tests;

public sealed class StoreFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> App { get; private set; } = null!;
    private string adminConnection = "";
    private readonly string database = "clothesstore_tests_" + Guid.NewGuid().ToString("N");
    public const string AdminEmail = "integration-admin@example.test";
    public const string Password = "Test-only!Password987";
    public async Task InitializeAsync()
    {
        var connection = Environment.GetEnvironmentVariable("CLOTHESSTORE_TEST_CONNECTION")
            ?? throw new InvalidOperationException("Set CLOTHESSTORE_TEST_CONNECTION to a PostgreSQL connection with CREATEDB permission. Tests use a separate temporary database.");
        var cs = new NpgsqlConnectionStringBuilder(connection) { Database = "postgres" };
        adminConnection = cs.ConnectionString;
        await using (var admin = new NpgsqlConnection(adminConnection))
        {
            await admin.OpenAsync();
            await using var command = new NpgsqlCommand("CREATE DATABASE \"" + database + "\"", admin);
            await command.ExecuteNonQueryAsync();
        }
        cs.Database = database;
        App = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Development");
            b.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = cs.ConnectionString,
                ["Database:ApplyMigrations"] = "true",
                ["Jwt:Key"] = "integration-tests-only-key-at-least-32-bytes-long",
                ["Jwt:Issuer"] = "ClothingStoreTests",
                ["Jwt:Audience"] = "ClothingStoreTests",
                ["Jwt:ExpirationMinutes"] = "30",
                ["SeedAdmin:Email"] = AdminEmail,
                ["SeedAdmin:Password"] = Password,
                ["Storage:Provider"] = "Local"
            }));
        });
        using var client = App.CreateClient();
        (await client.GetAsync("/health")).EnsureSuccessStatusCode();
    }
    public async Task DisposeAsync()
    {
        if (App is not null) await App.DisposeAsync();
        NpgsqlConnection.ClearAllPools();
        if (string.IsNullOrEmpty(adminConnection)) return;
        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        await using var command = new NpgsqlCommand("DROP DATABASE IF EXISTS \"" + database + "\" WITH (FORCE)", admin);
        await command.ExecuteNonQueryAsync();
    }
}
[CollectionDefinition("PostgreSQL", DisableParallelization = true)]
public sealed class DatabaseCollection : ICollectionFixture<StoreFixture>;
