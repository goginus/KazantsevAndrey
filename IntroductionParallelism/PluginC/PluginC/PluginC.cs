using PluginInterfaces;

namespace PluginC
{
    public class PluginC : IPlugin
    {
        public void Load()
        {
            throw new Exception("Error loading PluginC");
        }
    }
}