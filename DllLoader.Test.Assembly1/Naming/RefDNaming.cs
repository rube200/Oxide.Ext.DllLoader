using DllLoader.Test.Libs;
using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace Test.Naming
{
    [Info("RefDNaming", "Rube200", "1.0.0")]
    [Description("RefDNaming is for testing")]
    public class RefDNaming : DepTestPlugin
    {
        [PluginReference("RefENaming")]
        Plugin? plugin;

        public override Plugin? DepPlugin => plugin;
        public override string PluginName => "RefENaming";

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