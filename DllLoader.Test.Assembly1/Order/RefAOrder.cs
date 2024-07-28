using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAOrder", "Rube200", "1.0.0")]
    [Description("RefAOrder is for testing")]
    public class RefAOrder : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefBOrder;

        public override Plugin? DepPlugin => RefBOrder;
        public override string PluginName => nameof(RefBOrder);

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