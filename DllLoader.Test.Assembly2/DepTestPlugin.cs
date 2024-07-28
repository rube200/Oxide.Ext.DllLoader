using Oxide.Core.Plugins;
using System;

namespace DllLoader.Test.Libs
{
    public abstract class DepTestPlugin : TestPlugin
    {
        public abstract Plugin? DepPlugin { get; }
        public abstract string PluginName { get; }
        public virtual float DelayTime { get; } = 3f;

        protected override void Init()
        {
            base.Init();
            InitTest();
        }

        protected void InitTest(bool callOnTimer = true)
        {
            CheckPluginLoaded(callOnTimer);
        }

        protected override void Loaded()
        {
            base.Loaded();
            LoadedTest();
        }

        protected virtual void LoadedTest(bool callOnTimer = true)
        {
            CheckPluginLoaded(callOnTimer);
        }

        protected override void Unload()
        {
            base.Unload();
        }

        protected override void Shutdown()
        {
            base.Shutdown();
            CheckPluginLoaded();
        }

        protected override void Hotloading()
        {
            base.Hotloading();
            CheckPluginLoaded();
        }

        public virtual void CheckPluginLoaded(bool callOnTimer = true)
        {
            if (callOnTimer)
            {
                timer.Once(DelayTime, CallPluginRef);
            }
            else
            {
                CallPluginRef();
            }

            void CallPluginRef()
            {
                if (DepPlugin == null)
                    throw new Exception($"{PluginName} plugin ref ({PluginName}) is UNLOADED.");

                DepPlugin.Call(nameof(CallRef), SelfName);
            }
        }
    }
}
