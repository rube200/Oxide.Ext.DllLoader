using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefABCross", "Rube200", "1.0.0")]
    [Description("RefABCross is for testing")]
    public class RefABCross : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefAACross;

        public override Plugin? DepPlugin => RefAACross;
        public override string PluginName => nameof(RefAACross);
        public override float DelayTime => 5f;

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