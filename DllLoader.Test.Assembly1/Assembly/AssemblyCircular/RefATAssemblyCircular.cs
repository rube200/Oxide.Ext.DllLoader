using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefATAssemblyCircular", "Rube200", "1.0.0")]
    [Description("RefATAssemblyCircular is for testing")]
    public class RefATAssemblyCircular : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefAUAssemblyCircular;

        public override Plugin? DepPlugin => RefAUAssemblyCircular;

        public override string PluginName => nameof(RefAUAssemblyCircular);

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