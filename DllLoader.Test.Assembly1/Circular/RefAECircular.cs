using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAECircular", "Rube200", "1.0.0")]
    [Description("RefAECircular is for testing")]
    public class RefAECircular : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefAFCircular;

        public override Plugin? DepPlugin => RefAFCircular;
        public override string PluginName => nameof(RefAFCircular);

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