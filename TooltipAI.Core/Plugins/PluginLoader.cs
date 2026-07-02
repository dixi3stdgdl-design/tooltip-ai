using System.Reflection;
using TooltipAI.Core.Interfaces;

namespace TooltipAI.Core.Plugins;

/// <summary>
/// Loads tooltip plugins from DLLs in a plugins directory.
/// </summary>
public class PluginLoader
{
    private readonly string _pluginsPath;
    private readonly List<ITooltipProvider> _plugins = new();

    public IReadOnlyList<ITooltipProvider> Plugins => _plugins.AsReadOnly();

    public PluginLoader(string? pluginsPath = null)
    {
        _pluginsPath = pluginsPath
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TooltipAI", "plugins");
    }

    public void LoadPlugins()
    {
        if (!Directory.Exists(_pluginsPath))
        {
            Directory.CreateDirectory(_pluginsPath);
            return;
        }

        var dllFiles = Directory.GetFiles(_pluginsPath, "*.dll");

        foreach (var dllPath in dllFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllPath);
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(ITooltipProvider).IsAssignableFrom(t)
                                && !t.IsInterface
                                && !t.IsAbstract);

                foreach (var type in pluginTypes)
                {
                    var plugin = (ITooltipProvider)Activator.CreateInstance(type)!;
                    _plugins.Add(plugin);
                    Console.WriteLine($"[Plugin] Loaded: {plugin.Name} ({string.Join(", ", plugin.SupportedProcesses)})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Plugin] Failed to load {dllPath}: {ex.Message}");
            }
        }

        _plugins.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    public ITooltipProvider? FindProvider(string processName)
    {
        return _plugins.FirstOrDefault(p =>
            p.SupportedProcesses.Any(s =>
                s.Equals(processName, StringComparison.OrdinalIgnoreCase)));
    }
}
