using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefSDeep", "Rube200", "1.0.0")]
    [Description("RefSDeep is for testing")]
    public class RefSDeep : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefTDeep;

        public override Plugin? DepPlugin => RefTDeep;
        public override string PluginName => nameof(RefTDeep);

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