using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAMAssemblyOrder", "Rube200", "1.0.0")]
    [Description("RefAMAssemblyOrder is for testing")]
    public class RefAMAssemblyOrder : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefALAssemblyOrder;

        public override Plugin? DepPlugin => RefALAssemblyOrder;
        public override string PluginName => nameof(RefALAssemblyOrder);

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