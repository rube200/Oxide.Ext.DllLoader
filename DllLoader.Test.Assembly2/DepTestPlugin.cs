using Oxide.Core.Plugins;
using System;
using System.Diagnostics;

namespace Oxide.Plugins
{
    public abstract class DepTestPlugin : RustPlugin
    {
        protected abstract Plugin DepPlugin { get; }
        protected abstract string PuginName { get; }
        protected string SelfName => GetType().Name;


        protected virtual void Init()
        {
            Puts($"{SelfName} - Init");
            CheckPluginLoaded(false);
        }

        protected virtual void Loaded()
        {
            Puts($"{SelfName} - Loaded");
            CheckPluginLoaded();
        }

        protected virtual void OnServerInitialized()
        {
            Puts($"{SelfName} - OnServerInitialized");
            CheckPluginLoaded();
        }

        protected virtual void Unload()
        {
            Puts($"{SelfName} - Unload");
            CheckPluginLoaded();
        }

        protected virtual void Shutdown()
        {
            Puts($"{SelfName} - Shutdown");
            CheckPluginLoaded();
        }

        protected virtual void Hotloading()
        {
            Puts($"{SelfName} - Hotloading");
            CheckPluginLoaded();
        }

        protected virtual void CheckPluginLoaded(bool expectLoad = true)
        {
            if (expectLoad)
            {
                if (DepPlugin == null)
                    throw new Exception($"Plugin ref ({PuginName}) is UNLOADED.");

            }
            else
            {
                if (DepPlugin != null)
                    throw new Exception($"Plugin ref ({PuginName}) is LOADED.");
            }

            var invocatorName = new StackTrace().GetFrame(1).GetMethod().Name;
            timer.Once(3f, () =>
            {
                if (DepPlugin == null)
                    throw new Exception($"{invocatorName} plugin ref ({PuginName}) is UNLOADED.");

                DepPlugin?.Call(nameof(CallRef), SelfName);
            });
        }

        protected virtual void CallRef(string callerMsg)
        {
            Puts($"CallRef from: '{callerMsg}'");
        }
    }
}
