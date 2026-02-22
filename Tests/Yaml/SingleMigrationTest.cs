using System.Reflection;
using DocMigrator.Yaml;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

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

    deserialized.SchemaVersion.Should().Be(2);
    deserialized.Foo.Should().Be("foo-1");
  }

    [Fact]
    public async Task MigrationDeserializer_Increments_SchemaVersion()
    {
        // Given
        var migrationDeserializer = Setup();

        var original = @"
            schema_version: 0
            foo: original-value
            ";

        // When
        var deserialized = await migrationDeserializer.Deserialize<SingleMigrationClass>(original);

        // Then
        if (deserialized == null)
        {
            Assert.Fail("Failed to deserialize");
        }

        deserialized.SchemaVersion.Should().Be(2);
    }

    [Fact]
    public async Task MigrationDeserializer_Sets_NewValue()
    {
        // Given
        var migrationDeserializer = Setup();

        var original = @"
            schema_version: 0
            foo: original-value
            ";

        // When
        var deserialized = await migrationDeserializer.Deserialize<SingleMigrationClass>(original);

        // Then
        if (deserialized == null)
        {
            Assert.Fail("Failed to deserialize");
        }

        deserialized.Foo.Should().Be("foo-1");
    }

    [Fact]
    public async Task MigrationDeserializer_DoesNot_Apply_Old_Migration()
    {
        // Given
        var migrationDeserializer = Setup();

        var original = @"
            schema_version: 1
            foo: original-value
            ";

        // When
        var deserialized = await migrationDeserializer.Deserialize<SingleMigrationClass>(original);

        // Then
        if (deserialized == null)
        {
            Assert.Fail("Failed to deserialize");
        }

        deserialized.Foo.Should().Be("original-value");
    }

    [Fact]
    public async Task MigrationDeserializer_Does_Change_Type()
    {
        // Given
        var migrationDeserializer = Setup();

        var original = @"
            schema_version: 1
            foo: original-value
            runs_on: host1
            ";

        // When
        var deserialized = await migrationDeserializer.Deserialize<SingleMigrationClass>(original);

        // Then
        if (deserialized == null)
        {
            Assert.Fail("Failed to deserialize");
        }

        deserialized.RunsOn.Should().Equal(new List<string>{"host1"});
    }
}
