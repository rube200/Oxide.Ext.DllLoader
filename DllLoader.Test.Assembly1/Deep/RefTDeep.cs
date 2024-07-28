using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefTDeep", "Rube200", "1.0.0")]
    [Description("RefTDeep is for testing")]
    public class RefTDeep : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefUDeep;

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