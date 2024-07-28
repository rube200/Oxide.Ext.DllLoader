using DllLoader.Test.Libs;
using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace Test.Namespace
{
    [Info("RefGNamespace", "Rube200", "1.0.0")]
    [Description("RefGNamespace is for testing")]
    public class RefGNamespace : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefHNamespace;

        public override Plugin? DepPlugin => RefHNamespace;
        public override string PluginName => nameof(RefHNamespace);

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