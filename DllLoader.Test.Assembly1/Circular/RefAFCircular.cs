using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAFCircular", "Rube200", "1.0.0")]
    [Description("RefAFCircular is for testing")]
    public class RefAFCircular : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefAECircular;

        public override Plugin? DepPlugin => RefAECircular;
        public override string PluginName => nameof(RefAECircular);

        protected override void Init()
        {
            base.Init();
        }

        protected override void Loaded()
        {
            base.Loaded();
        }

        protected override void LoadedTest(bool callOnTimer = true)
        {
            base.LoadedTest(true);
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