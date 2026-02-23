using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.Extensions.Logging;

namespace DocMigrator.Yaml;

/// <summary>
///   Base class for deserializing Yaml documents with schema migrations
/// </summary>
/// <typeparam name="T">The type to deserialize.</typeparam>
public abstract class YamlMigrationDeserializer<T> where T : class
{
    private readonly ILogger<YamlMigrationDeserializer<T>> _logger;
    private readonly DeserializerBuilder _deserializerBuilder;
    private readonly SerializerBuilder _serializerBuilder;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlMigrationDeserializer{T}" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="migrations">The list of migration functions.</param>
    /// <param name="deserializerBuilder"></param>
    /// <param name="serializerBuilder"></param>
    protected YamlMigrationDeserializer(IServiceProvider serviceProvider,
            ILogger<YamlMigrationDeserializer<T>> logger,
            IReadOnlyList<Func<IServiceProvider, Dictionary<object, object>, ValueTask>> migrations,
            DeserializerBuilder? deserializerBuilder = null, 
            SerializerBuilder? serializerBuilder = null)
    {
      _serviceProvider = serviceProvider;
      _logger = logger;
      _deserializerBuilder = deserializerBuilder ?? new DeserializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .IgnoreUnmatchedProperties();
      _serializerBuilder = serializerBuilder ?? new SerializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance);
      Migrations = migrations;
    }

    /// <summary>
    ///     Gets the list of migration functions.
    /// </summary>
    public IReadOnlyList<Func<IServiceProvider, Dictionary<object, object>, ValueTask>> Migrations { get; }

    /// <summary>
    ///     Gets the application schema version.
    /// </summary>
    public int AppSchemaVersion => Migrations.Count;

    /// <summary>
    ///     Deserializes the specified Yaml string and applies migrations.
    /// </summary>
    /// <param name="yaml">The Yaml string to deserialize.</param>
    /// <returns>A <see cref="ValueTask{T}" /> representing the asynchronous operation.</returns>
    public async ValueTask<T?> Deserialize(string yaml)
    {
        var deserializer = _deserializerBuilder.Build();

        var obj = deserializer.Deserialize<Dictionary<object, object>>(yaml);

        if (obj is null)
        {
            obj = new Dictionary<object, object>();
        }

        try
        {
            // Schema version defaults to 0
            var docSchemaVersion = 0;
            if (obj != null && obj.TryGetValue("schemaVersion", out var schema_version))
            {
              Type t = schema_version.GetType();
              switch (schema_version)
              {
                case int v:
                  docSchemaVersion = v;
                  break;
                case string v:
                  if (Int32.TryParse(v, out int j))
                    {
                        docSchemaVersion = j;
                    }
                    else
                    {
                        _logger.LogError("String could not be parsed.");
                        return null;
                    }
                  break;
                default:
                  // Using v0
                  _logger.LogWarning("Invalid schema_version type: {0}", t);
                  break;
              }
            }
            else
            {
              _logger.LogWarning("No schema_version specified, defaulting to 0");
            }

            if (docSchemaVersion >= AppSchemaVersion)
            {
                // No migrations to be applied
                return ConvertTo(obj!);
            }

            // Apply migrations
            for (var i = docSchemaVersion; i < AppSchemaVersion; i++)
            {
                await Migrations[i](_serviceProvider, obj!);
            }

            // Update schema version
            obj!["schemaVersion"] = AppSchemaVersion;

            return ConvertTo(obj!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize {type}", nameof(T));
            return null;
        }
    }

    /// <summary>
    ///   Converts a migrated YAML object graph into a strongly typed instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="migratedObject">The migrated YAML object graph.</param>
    /// <returns>An instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="migratedObject"/> is null.
    /// </exception>
    /// <exception cref="YamlDotNet.Core.YamlException">
    ///   Thrown when the object cannot be deserialized into <typeparamref name="T"/>.
    /// </exception>
    public T ConvertTo(Dictionary<object, object> migratedObject)
    {
        if (migratedObject is null)
            throw new ArgumentNullException(nameof(migratedObject));
        
        var serializer = _serializerBuilder.Build();
        var deserializer = _deserializerBuilder.Build();

        var yaml = serializer.Serialize(migratedObject);

        return deserializer.Deserialize<T>(yaml);
    }
}
