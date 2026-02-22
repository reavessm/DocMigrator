using System.Reflection;
using DocMigrator.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Tests.Yaml;

public class SingleMigrationTests
{
  public YamlMigrator Setup()
  {
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddYamlMigrator(Assembly.GetExecutingAssembly());
    var scope = services.BuildServiceProvider().CreateScope();
    return scope.ServiceProvider.GetRequiredService<YamlMigrator>();
  }

  [Fact]
  public async Task MigrationDeserializer_Creates_Missing_Properties()
  {
    // Given
    var migrationDeserializer = Setup();

    // When
    var deserialized = await migrationDeserializer.Deserialize<SingleMigrationClass>("");

    // Then
    if (deserialized == null)
    {
      Assert.Fail("Failed to deserialize");
    }

    deserialized.SchemaVersion.ShouldBe(2);
    deserialized.Foo.ShouldBe("foo-1");
  }

    [Fact]
    public async Task MigrationDeserializer_Increments_SchemaVersion()
    {
        // Given
        var migrationDeserializer = Setup();

        var original = @"
            schemaVersion: 0
            foo: original-value
            ";

        // When
        var deserialized = await migrationDeserializer.Deserialize<SingleMigrationClass>(original);

        // Then
        if (deserialized == null)
        {
            Assert.Fail("Failed to deserialize");
        }

        deserialized.SchemaVersion.ShouldBe(2);
    }

    [Fact]
    public async Task MigrationDeserializer_Sets_NewValue()
    {
        // Given
        var migrationDeserializer = Setup();

        var original = @"
            schemaVersion: 0
            foo: original-value
            ";

        // When
        var deserialized = await migrationDeserializer.Deserialize<SingleMigrationClass>(original);

        // Then
        if (deserialized == null)
        {
            Assert.Fail("Failed to deserialize");
        }

        deserialized.Foo.ShouldBe("foo-1");
    }

    [Fact]
    public async Task MigrationDeserializer_DoesNot_Apply_Old_Migration()
    {
        // Given
        var migrationDeserializer = Setup();

        var original = @"
            schemaVersion: 1
            foo: original-value
            ";

        // When
        var deserialized = await migrationDeserializer.Deserialize<SingleMigrationClass>(original);

        // Then
        if (deserialized == null)
        {
            Assert.Fail("Failed to deserialize");
        }

        deserialized.Foo.ShouldBe("original-value");
    }

    [Fact]
    public async Task MigrationDeserializer_Does_Change_Type()
    {
        // Given
        var migrationDeserializer = Setup();

        var original = @"
            schemaVersion: 1
            foo: original-value
            runsOn: host1
            ";

        // When
        var deserialized = await migrationDeserializer.Deserialize<SingleMigrationClass>(original);

        // Then
        if (deserialized == null)
        {
            Assert.Fail("Failed to deserialize");
        }

        deserialized.RunsOn.ShouldBeEquivalentTo(new List<string>{"host1"});
    }

    [Fact]
    public async Task MigrationDeserializer_Ignores_Extra_Fields()
    {
        // Given
        var migrationDeserializer = Setup();

        var original = @"
            schemaVersion: 1
            foo: original-value
            runsOn: host1
            doesNotExist: blah
            ";

        // When
        var deserialized = await migrationDeserializer.Deserialize<SingleMigrationClass>(original);

        // Then
        if (deserialized == null)
        {
            Assert.Fail("Failed to deserialize");
        }

        deserialized.RunsOn.ShouldBeEquivalentTo(new List<string>{"host1"});
    }
}
