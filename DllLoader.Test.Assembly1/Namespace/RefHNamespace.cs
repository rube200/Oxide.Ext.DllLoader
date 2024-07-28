using DllLoader.Test.Libs;
using Oxide.Plugins;

namespace Test.Namespace
{
    [Info("RefHNamespace", "Rube200", "1.0.0")]
    [Description("RefHNamespace is for testing")]
    public class RefHNamespace : TestPlugin
    {
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