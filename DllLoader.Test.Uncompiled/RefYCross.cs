using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefYCross", "Rube200", "1.0.0")]
    [Description("RefYCross is for testing")]
    public class RefYCross : DepTestPlugin
    {
        [PluginReference]
        Plugin RefZCross;

        protected override Plugin DepPlugin => RefZCross;
        protected override string PuginName => nameof(RefZCross);

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