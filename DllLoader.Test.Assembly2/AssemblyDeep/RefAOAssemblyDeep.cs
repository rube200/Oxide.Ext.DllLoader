using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAOAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefAOAssemblyDeep is for testing")]
    public class RefAOAssemblyDeep : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefAPAssemblyDeep;

        public override Plugin? DepPlugin => RefAPAssemblyDeep;
        public override string PluginName => nameof(RefAPAssemblyDeep);

        protected override void Init()
        {
            base.Init();
        }

        protected override void Loaded()
        {
            base.Loaded();
        }

        protected override void LoadedTest(bool callOnTimer = true)
        {
            base.LoadedTest(true);
        }

        protected override void Unload()
        {
            base.Unload();
        }

        protected override void Shutdown()
        {
            base.Shutdown();
        }

        protected override void Hotloading()
        {
            base.Hotloading();
        }

        protected override void CallRef(string callerMsg)
        {
            base.CallRef(callerMsg);
        }
    }
}