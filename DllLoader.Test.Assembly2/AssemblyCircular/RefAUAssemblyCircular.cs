using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAUAssemblyCircular", "Rube200", "1.0.0")]
    [Description("RefAUAssemblyCircular is for testing")]
    public class RefAUAssemblyCircular : DepTestPlugin
    {
        [PluginReference] 
        Plugin? RefATAssemblyCircular;

        public override Plugin? DepPlugin => RefATAssemblyCircular;
        public override string PluginName => nameof(RefATAssemblyCircular);

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