using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAICircular", "Rube200", "1.0.0")]
    [Description("RefAICircular is for testing")]
    public class RefAICircular : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefAGCircular;

        public override Plugin? DepPlugin => RefAGCircular;
        public override string PluginName => nameof(RefAGCircular);

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