using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    public abstract class TestPlugin : RustPlugin
    {
        protected string SelfName => GetType().Name;


        protected virtual void Init()
        {
            Puts($"{SelfName} - Init");
        }

        protected virtual void Loaded()
        {
            Puts($"{SelfName} - Loaded");
        }

        protected virtual void OnServerInitialized()
        {
            Puts($"{SelfName} - OnServerInitialized");
        }

        protected virtual void Unload()
        {
            Puts($"{SelfName} - Unload");
        }

        protected virtual void Shutdown()
        {
            Puts($"{SelfName} - Shutdown");
        }

        protected virtual void Hotloading()
        {
            Puts($"{SelfName} - Hotloading");
        }

        protected virtual void CallRef(string callerMsg)
        {
            Puts($"CallRef from: '{callerMsg}'");
        }
    }
}
