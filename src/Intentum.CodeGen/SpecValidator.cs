namespace Intentum.CodeGen;

/// <summary>
/// Validates YAML/JSON spec files for CodeGen.
/// </summary>
public static class SpecValidator
{
    /// <summary>
    /// Validates a spec model and returns validation errors.
    /// </summary>
    public static IReadOnlyList<string> Validate(SpecModel spec)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(spec.Namespace))
            errors.Add("Namespace is required");
        if (spec.Features == null || spec.Features.Count == 0)
            errors.Add("At least one feature is required");
        foreach (var feature in spec.Features ?? [])
        {
            ValidateFeature(feature, errors);
        }
        return errors;
    }

    private static void ValidateFeature(FeatureSpec feature, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(feature.Name))
            errors.Add("Feature name is required");
        foreach (var command in feature.Commands ?? [])
            ValidateCommand(command, feature.Name, errors);
        foreach (var query in feature.Queries ?? [])
            ValidateQuery(query, feature.Name, errors);
    }

    private static void ValidateCommand(CommandSpec command, string featureName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            errors.Add($"Command name is required in feature '{featureName}'");
        foreach (var prop in command.Properties ?? [])
        {
            if (string.IsNullOrWhiteSpace(prop.Name))
                errors.Add($"Property name is required in command '{command.Name}'");
            if (string.IsNullOrWhiteSpace(prop.Type))
                errors.Add($"Property type is required for '{prop.Name}' in command '{command.Name}'");
        }
    }

    private static void ValidateQuery(QuerySpec query, string featureName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(query.Name))
            errors.Add($"Query name is required in feature '{featureName}'");
        foreach (var prop in query.Properties ?? [])
        {
            if (string.IsNullOrWhiteSpace(prop.Name))
                errors.Add($"Property name is required in query '{query.Name}'");
            if (string.IsNullOrWhiteSpace(prop.Type))
                errors.Add($"Property type is required for '{prop.Name}' in query '{query.Name}'");
        }
    }
}
