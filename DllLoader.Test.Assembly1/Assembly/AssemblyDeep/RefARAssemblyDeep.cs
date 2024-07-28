using DllLoader.Test.Libs;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefARAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefARAssemblyDeep is for testing")]
    public class RefARAssemblyDeep : DepTestPlugin
    {
        [PluginReference]
        Plugin? RefASAssemblyDeep;

        public override Plugin? DepPlugin => RefASAssemblyDeep;
        public override string PluginName => nameof(RefASAssemblyDeep);

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