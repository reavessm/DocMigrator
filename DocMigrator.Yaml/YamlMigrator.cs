using YamlDotNet.Core;
using Microsoft.Extensions.DependencyInjection;

namespace DocMigrator.Yaml;

/// <summary>
///   Apply migrations on deserialization of a Yaml document
/// </summary>
public class YamlMigrator
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlMigrator" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public YamlMigrator(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     Deserializes the specified Yaml document into an instance of type <typeparamref name="T" /> and applies migrations.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="document">The Yaml document to deserialize.</param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> representing the asynchronous operation, containing the deserialized
    ///     object.
    /// </returns>
    public ValueTask<T?> Deserialize<T>(string document) where T : class
    {
        var migrator = _serviceProvider.GetRequiredService<YamlMigrationDeserializer<T>>();
        return migrator.Deserialize(document);
    }
}
