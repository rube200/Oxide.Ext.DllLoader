using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAGCircular", "Rube200", "1.0.0")]
    [Description("RefAGCircular is for testing")]
    public class RefAGCircular : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefAHCircular;

        public override Plugin? DepPlugin => RefAHCircular;
        public override string PluginName => nameof(RefAHCircular);

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