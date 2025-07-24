namespace Reglamentator.Bot.Extensions;
public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration config, string key)
    {
        var value = config[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuration value for '{key}' is required");
        }
        return value;
    }
}