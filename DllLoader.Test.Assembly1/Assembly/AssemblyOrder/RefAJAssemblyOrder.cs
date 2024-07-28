using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAJAssemblyOrder", "Rube200", "1.0.0")]
    [Description("RefAJAssemblyOrder is for testing")]
    public class RefAJAssemblyOrder : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefAKAssemblyOrder;

        public override Plugin? DepPlugin => RefAKAssemblyOrder;
        public override string PluginName => nameof(RefAKAssemblyOrder);

        protected override void Init()
        {
            base.Init();
        }

        protected override void Loaded()
        {
            base.Loaded();
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