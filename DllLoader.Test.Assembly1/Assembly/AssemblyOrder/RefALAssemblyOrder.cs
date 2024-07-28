using DllLoader.Test.Libs;

namespace Oxide.Plugins
{
    [Info("RefALAssemblyOrder", "Rube200", "1.0.0")]
    [Description("RefALAssemblyOrder is for testing")]
    public class RefALAssemblyOrder : TestPlugin
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