using System.Reflection;
using DocMigrator.Bson;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;

namespace Tests.Bson;

public class BsonMigrationTests
{
    public BsonMigrator Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBsonMigrator(Assembly.GetExecutingAssembly());
        var scope = services.BuildServiceProvider().CreateScope();
        return scope.ServiceProvider.GetRequiredService<BsonMigrator>();
    }

    [Fact]
    public async Task MigrationDeserializer_Creates_Missing_Properties()
    {
        // Given
        var migrationDeserializer = Setup();
        var bsonDocument = new BsonDocument();

        // When
        var deserialized = await migrationDeserializer.Deserialize<BsonMigrationClass>(bsonDocument);

        // Then
        if (deserialized == null)
        {
            Assert.Fail("Failed to deserialize");
        }

        deserialized.SchemaVersion.ShouldBe(1);
        deserialized.Foo.ShouldBe("foo-1");
    }
}