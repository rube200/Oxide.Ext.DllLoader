using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefWDeep", "Rube200", "1.0.0")]
    [Description("RefWDeep is for testing")]
    public class RefWDeep : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefVDeep;

        public override Plugin? DepPlugin => RefVDeep;
        public override string PluginName => nameof(RefVDeep);

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