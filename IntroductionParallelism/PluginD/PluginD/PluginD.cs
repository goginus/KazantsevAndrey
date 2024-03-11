using PluginInterfaces;

namespace PluginD
{
    public class PluginD : IPlugin
    {
        public void Load()
        {
            Console.WriteLine("PluginD is loaded");
        }
    }
}