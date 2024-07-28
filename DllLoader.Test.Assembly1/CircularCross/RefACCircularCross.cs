using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefACCircularCross", "Rube200", "1.0.0")]
    [Description("RefACCircularCross is for testing")]
    public class RefACCircularCross : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefADCircularCross;

        public override Plugin? DepPlugin => RefADCircularCross;
        public override string PluginName => nameof(RefADCircularCross);
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