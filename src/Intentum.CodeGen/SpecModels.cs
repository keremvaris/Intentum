namespace Intentum.CodeGen;

public class SpecModel
{
    public string? Namespace { get; set; }
    public List<FeatureSpec> Features { get; set; } = [];
}

public class FeatureSpec
{
    public string Name { get; set; } = "";
    public List<CommandSpec>? Commands { get; set; }
    public List<QuerySpec>? Queries { get; set; }
}

public class CommandSpec
{
    public string Name { get; set; } = "";
    public List<PropertySpec>? Properties { get; set; }
}

public class QuerySpec
{
    public string Name { get; set; } = "";
    public List<PropertySpec>? Properties { get; set; }
}

public class PropertySpec
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "string";
    public PropertySpec() { }
    public PropertySpec(string name, string type) { Name = name; Type = type; }
}
