using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefXDee", "Rube200", "1.0.0")]
    [Description("RefXDeep is for testing")]
    public class RefXDeep : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefWDeep;

        public override Plugin? DepPlugin => RefWDeep;
        public override string PluginName => nameof(RefWDeep);

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