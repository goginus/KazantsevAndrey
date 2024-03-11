using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using PluginInterfaces;

class PluginWrapper
{
    public IPlugin Plugin { get; }
    public int AttemptCount { get; private set; }

    public PluginWrapper(IPlugin plugin)
    {
        Plugin = plugin;
        AttemptCount = 0;
    }

    public bool CanRetry()
    {
        return ++AttemptCount <= 3;
    }
}

class Program
{
    private static ConcurrentQueue<PluginWrapper> pluginsQueue = new ConcurrentQueue<PluginWrapper>();
    private static ConcurrentQueue<PluginWrapper> retryQueue = new ConcurrentQueue<PluginWrapper>();
    private static bool initialScanCompleted = false;

    static void Main(string[] args)
    {
        var pluginDirectory = "C:\\HomeWork\\KazantsevAndrey\\IntroductionParallelism\\Plugins\\net6.0";
        Task loadTask = Task.Run(() => LoadPlugins(pluginDirectory));
        Task processTask = Task.Run(() => ProcessPlugins());
 
        Task.WaitAll(loadTask, processTask);

        Console.WriteLine("All plugins processed.");
    }

    static void LoadPlugins(string pluginPath)
    {
        foreach (var dll in Directory.GetFiles(pluginPath, "*.dll"))
        {
            try
            {
                var assembly = Assembly.LoadFrom(dll);
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                    {
                        var plugin = Activator.CreateInstance(type) as IPlugin;
                        if (plugin != null)
                        {
                            pluginsQueue.Enqueue(new PluginWrapper(plugin));
                            Console.WriteLine($"{type.Name} loaded and added to the queue.");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to create an instance of type {type.Name}.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load assembly: {dll}, error: {ex.Message}");
            }
        }

        initialScanCompleted = true;
    }

    static void ProcessPlugins()
    {
        while (!initialScanCompleted || !pluginsQueue.IsEmpty || !retryQueue.IsEmpty)
        {
            if (!initialScanCompleted || !pluginsQueue.IsEmpty)
            {
                TryProcessPluginFromQueue(pluginsQueue);
            }
            
            if (initialScanCompleted && pluginsQueue.IsEmpty && !retryQueue.IsEmpty)
            {
                TryProcessPluginFromQueue(retryQueue);
            }
        }
    }

    static void TryProcessPluginFromQueue(ConcurrentQueue<PluginWrapper> queue)
    {
        if (queue.TryDequeue(out var pluginWrapper))
        {
            try
            {
                pluginWrapper.Plugin.Load();
                Console.WriteLine($"Plugin {pluginWrapper.Plugin.GetType().Name} loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading plugin {pluginWrapper.Plugin.GetType().Name}: {ex.Message}");
                if (pluginWrapper.CanRetry())
                {
                    retryQueue.Enqueue(pluginWrapper);
                    Console.WriteLine($"Re-enqueued plugin {pluginWrapper.Plugin.GetType().Name} for another attempt.");
                }
                else
                {
                    Console.WriteLine($"Max attempts reached for {pluginWrapper.Plugin.GetType().Name}, not re-enqueuing.");
                }
            }
        }
    }
}