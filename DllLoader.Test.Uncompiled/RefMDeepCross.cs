using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefMDeepCross", "Rube200", "1.0.0")]
    [Description("RefMDeepCross is for testing")]
    public class RefMDeepCross : DepTestPlugin
    {
        [PluginReference]
        Plugin RefNDeepCross;

        protected override Plugin DepPlugin => RefNDeepCross;
        protected override string PuginName => nameof(RefNDeepCross);

        protected override void Init()
        {
            base.Init();
        }

        protected override void Loaded()
        {
            base.Loaded();
        }

        protected override void OnServerInitialized()
        {
            base.OnServerInitialized();
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
    }
}