using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Common;

/// <summary>
/// Helpers for reading and writing JSON-serialized objects to disk with
/// consistent formatting and error handling.
/// </summary>
public static class JsonFile
{
    /// <summary>Shared options producing human-readable, indented JSON.</summary>
    public static readonly JsonSerializerOptions IndentedOptions = new() { WriteIndented = true };

    /// <summary>
    /// Loads and deserializes <typeparamref name="T"/> from <paramref name="path"/>.
    /// Returns the result of <paramref name="fallback"/> when the file is
    /// missing, empty, or cannot be read/deserialized.
    /// </summary>
    public static T Load<T>(
        string path,
        Func<T> fallback,
        ILogger? logger = null,
        JsonSerializerOptions? options = null,
        string? description = null)
    {
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var value = JsonSerializer.Deserialize<T>(json, options);
                if (value is not null)
                    return value;
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to load {Description} from {Path}",
                description ?? typeof(T).Name, path);
        }
        return fallback();
    }

    /// <summary>
    /// Serializes <paramref name="value"/> and writes it to <paramref name="path"/>.
    /// Uses <see cref="IndentedOptions"/> unless <paramref name="options"/> is supplied.
    /// Returns <c>false</c> when writing fails.
    /// </summary>
    public static bool Save<T>(
        string path,
        T value,
        ILogger? logger = null,
        JsonSerializerOptions? options = null,
        string? description = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, options ?? IndentedOptions);
            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to save {Description} to {Path}",
                description ?? typeof(T).Name, path);
            return false;
        }
    }
}
