using PluginInterfaces;

namespace PluginB
{
    public class PluginB : IPlugin
    {
        public void Load()
        {
            Console.WriteLine("PluginB is loaded");
        }
    }
}