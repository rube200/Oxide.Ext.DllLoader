using DllLoader.Test.Libs;

namespace Oxide.Plugins
{
    [Info("RefAPAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefAPAssemblyDeep is for testing")]
    public class RefAPAssemblyDeep : TestPlugin
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