using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefNDeepCross", "Rube200", "1.0.0")]
    [Description("RefNDeepCross is for testing")]
    public class RefNDeepCross : DepTestPlugin
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