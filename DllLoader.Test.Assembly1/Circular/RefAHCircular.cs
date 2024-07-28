using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAHCircular", "Rube200", "1.0.0")]
    [Description("RefAHCircular is for testing")]
    public class RefAHCircular : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefAICircular;

        public override Plugin? DepPlugin => RefAICircular;
        public override string PluginName => nameof(RefAICircular);

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