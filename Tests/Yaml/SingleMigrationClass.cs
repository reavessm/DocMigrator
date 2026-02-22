using YamlDotNet.Serialization;

namespace Tests.Yaml;

public class SingleMigrationClass
{
  public int SchemaVersion { get; set; }
  public string Foo { get; set; } = string.Empty;
  public List<string> RunsOn { get; set; } = new List<string>();
}
