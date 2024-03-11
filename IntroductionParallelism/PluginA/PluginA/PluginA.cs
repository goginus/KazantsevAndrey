using PluginInterfaces;

namespace PluginA
{
    public class PluginA : IPlugin
    {
        public void Load()
        {
            Console.WriteLine("PluginA is loaded");
        }
    }
}