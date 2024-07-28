using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefRDeepCross", "Rube200", "1.0.0")]
    [Description("RefRDeepCross is for testing")]
    public class RefRDeepCross : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefQDeepCross;

        public override Plugin? DepPlugin => RefQDeepCross;
        public override string PluginName => nameof(RefQDeepCross);
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