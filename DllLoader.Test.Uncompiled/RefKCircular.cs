using Oxide.Core.Plugins;
using System;
using System.Diagnostics;

namespace Oxide.Plugins
{
    [Info("RefKCircular", "Rube200", "1.0.0")]
    [Description("RefKCircular is for testing")]
    public class RefKCircular : RustPlugin
    {
        protected string SelfName => GetType().Name;

        [PluginReference]
        Plugin? RefJCircular;

        public Plugin? DepPlugin => RefJCircular;
        public string PluginName => nameof(RefJCircular);

        protected void Init()
        {
            Puts($"{SelfName} - Init");
            CheckPluginLoaded();
        }

        protected void Loaded()
        {
            Puts($"{SelfName} - Loaded");
            CheckPluginLoaded();
        }

        protected void Unload()
        {
            Puts($"{SelfName} - Unload");
        }

        protected void Shutdown()
        {
            Puts($"{SelfName} - Shutdown");
            CheckPluginLoaded();
        }

        protected void Hotloading()
        {
            Puts($"{SelfName} - Hotloading");
            CheckPluginLoaded();
        }

        protected void CallRef(string callerMsg)
        {
            Puts($"CallRef from: '{callerMsg}'");
        }

        public virtual void CheckPluginLoaded(bool callOnTimer = true)
        {
            var invocatorName = new StackTrace().GetFrame(1).GetMethod().Name;
            if (callOnTimer)
            {
                timer.Once(3f, CallPluginRef);
            }
            else
            {
                CallPluginRef();
            }

            void CallPluginRef()
            {
                if (DepPlugin == null)
                    throw new Exception($"{invocatorName} plugin ref ({PluginName}) is UNLOADED.");

                DepPlugin.Call(nameof(CallRef), SelfName);
            }
        }
    }
}