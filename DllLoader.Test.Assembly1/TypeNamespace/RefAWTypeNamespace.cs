using DllLoader.Test.Libs;
using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace Test.Namespace
{
    [Info("RefAWTypeNamespace", "Rube200", "1.0.0")]
    [Description("RefAWTypeNamespace is for testing")]
    public class RefAWTypeNamespace : DepTestPlugin
    {
        [PluginReference]
        RefUDeep? RefUDeep;

        public override Plugin? DepPlugin => RefUDeep;
        public override string PluginName => nameof(RefUDeep);

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